using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using Color = Microsoft.Xna.Framework.Color;

namespace Staxel.Trace {
    public class TraceRecorder {
        const int RecordSize = 12; // must match struct Entry's size
        const int RingSize = 10000000;
        const int RingFlushSize = 1000000;
        const int WriteBufferSize = 64 * 1024;
        private const int QueueSize = 100;
        static SpinLock _lock = new SpinLock();
        static int _ringTail;
        static int _ringHead;
        static FileStream _file;
        static long _epoch;
        static long _tickRation;


        readonly static Queue<long> Averages = new Queue<long>(QueueSize);
        private static long _lastDuration;
        public static double AverageDuration;
        public static double FrameDuration;
        [Conditional("TRACE")]
        public static void CalcInterval() {
            var duration = Stopwatch.GetTimestamp() - _lastDuration;
            Averages.Enqueue(duration);
            if(Averages.Count > QueueSize)
                Averages.Dequeue();
            AverageDuration = Averages.Average() / Stopwatch.Frequency;
            FrameDuration = (double)duration / Stopwatch.Frequency;

            _lastDuration = Stopwatch.GetTimestamp();
        }
        [Conditional("TRACE")]
        public static void Start() {
            var lockTaken = false;
            _lock.Enter(ref lockTaken);
            _epoch = Stopwatch.GetTimestamp();
            _tickRation = 1073741824000000L / Stopwatch.Frequency;
            _file = new FileStream(DateTime.Now.ToString("yyyyMMddHHmmss", CultureInfo.InvariantCulture) + ".staxeltrace", FileMode.CreateNew);

            _lock.Exit();
        }

        [Conditional("TRACE")]
        public static void Stop() {
            Flush(true);
            var lockTaken = false;
            _lock.Enter(ref lockTaken);
            if (_file != null) {
                _file.Close();
                _file = null;
            }
            _lock.Exit();
        }

        [Conditional("TRACE")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Enter(TraceKey trace){
            if(Gearset.Gs.Console != null && Gearset.Gs.Console.Profiler.FrameStarted)
                Gearset.Gs.Console.BeginMark(trace.Code, new Color(trace.Color.R, trace.Color.G, trace.Color.B));
            trace.LiveDuration = Stopwatch.GetTimestamp();
            if (_file == null)
                return;
            TraceRecord traceRecord;
            traceRecord.Thread = Thread.CurrentThread.ManagedThreadId;
            traceRecord.Timestamp = (int)(((Stopwatch.GetTimestamp() - _epoch) * _tickRation) >> 30);
            traceRecord.Scope = (trace.Id << 1) | 1;
            var lockTaken = false;
            _lock.Enter(ref lockTaken);
            RingBuffer[_ringHead++] = traceRecord;
            if (_ringHead == RingSize)
                _ringHead = 0;
            if (_ringTail == _ringHead) {
                _ringTail++;
                if (_ringTail > RingSize)
                    _ringTail = 0;
            }
            _lock.Exit();
        }

        [Conditional("TRACE")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Leave(TraceKey trace){
            if(Gearset.Gs.Console != null && Gearset.Gs.Console.Profiler.FrameStarted)
                Gearset.Gs.Console.EndMark(trace.Code);
            trace.LiveDuration = Stopwatch.GetTimestamp() - trace.LiveDuration;
            if (_file == null)
                return;
            TraceRecord traceRecord;
            traceRecord.Thread = Thread.CurrentThread.ManagedThreadId;
            traceRecord.Timestamp = (int)(((Stopwatch.GetTimestamp() - _epoch) * _tickRation) >> 30);
            traceRecord.Scope = (trace.Id << 1) | 0;
            var lockTaken = false;
            _lock.Enter(ref lockTaken);
            RingBuffer[_ringHead++] = traceRecord;
            if (_ringHead == RingSize)
                _ringHead = 0;
            if (_ringTail == _ringHead) {
                _ringTail++;
                if (_ringTail > RingSize)
                    _ringTail = 0;
            }
            _lock.Exit();
        }

        [Conditional("TRACE")]
        public static unsafe void Flush(bool hard = false) {
            if (_file == null)
                return;
            var lockTaken = false;
            _lock.Enter(ref lockTaken);
            try {
                var entries = _ringHead - _ringTail;
                if (entries < 0)
                    entries += RingSize;
                var flush = hard | (entries >= RingFlushSize);
                if (flush) {
                    while (_ringHead != _ringTail) {
                        var limit = _ringHead;
                        if (limit < _ringTail)
                            limit = RingSize;
                        var length = limit - _ringTail;
                        if (length > WriteBuffer.Length / RecordSize)
                            length = WriteBuffer.Length / RecordSize;
                        var bytes = length * RecordSize;
                        {
                            fixed (TraceRecord* from = &RingBuffer[_ringTail])
                            fixed (byte* to = &WriteBuffer[0]) {
                                UnsafeNativeMethods.MoveMemory(to, from, bytes);
                            }
                        }
                        _file.Write(WriteBuffer, 0, bytes);
                        _ringTail += length;
                        if (_ringTail == RingSize)
                            _ringTail = 0;
                    }
                    if (hard)
                        _file.Flush();
                }
            }
            finally {
                _lock.Exit();
            }
        }

        static unsafe TraceRecord[] LoadRaw(string file) {
            var bytes = File.ReadAllBytes(file);
            var entries = bytes.Length / RecordSize;
            var result = new TraceRecord[entries];
            if (entries == 0)
                return result;
            fixed (byte* from = &bytes[0])
            fixed (TraceRecord* to = &result[0]) {
                UnsafeNativeMethods.MoveMemory(to, from, entries * RecordSize);
            }
            return result;
        }

        public static TraceEvent[] Load(string file, IEnumerable<TraceKey> keys) {
            var keysMap = new Dictionary<int, TraceKey>();
            foreach (var key in keys)
                keysMap.Add(key.Id, key);
            var entries = LoadRaw(file);
            long prevTimestamp = 0;
            var entriesLimit = 0;
            for (var i = 0; i < entries.Length; ++i) {
                var entry = entries[i];
                if (entry.Timestamp < prevTimestamp)
                    break; // overflow/loop around
                prevTimestamp = entry.Timestamp;
                entriesLimit++;
            }
            var result = new TraceEvent[entriesLimit];
            for (var i = 0; i < entriesLimit; ++i) {
                var entry = entries[i];
                TraceEvent e;
                e.Timestamp = entry.Timestamp;
                e.Thread = entry.Thread;
                e.Enter = (entry.Scope & 1) == 1;
                e.Trace = keysMap[entry.Scope >> 1];
                result[i] = e;
            }
            return result;
        }

        public struct TraceEvent {
            public bool Enter;
            public int Thread;
            public int Timestamp;
            public TraceKey Trace;
        }

        [StructLayout(LayoutKind.Explicit, Size = 12, Pack = 1)]
        struct TraceRecord {
            [FieldOffset(0)] public int Timestamp;
            [FieldOffset(4)] public int Thread;
            [FieldOffset(8)] public int Scope;
        }

        class UnsafeNativeMethods {
            [DllImport("Kernel32.dll", EntryPoint = "RtlMoveMemory", SetLastError = false)]
            internal static extern unsafe void MoveMemory(void* dest, void* src, int size);
        }

        static readonly TraceRecord[] RingBuffer = new TraceRecord[RingSize];
        static readonly byte[] WriteBuffer = new byte[WriteBufferSize];
    }
}
