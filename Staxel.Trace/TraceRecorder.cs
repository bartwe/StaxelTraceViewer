using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;

namespace Staxel.Trace {
    public sealed class TraceRecorder {
        const int RecordSize = 16; // must match struct Entry's size
        const int RingSize = 10000000;
        const int RingFlushSize = 1000000;
        const int WriteBufferSize = 64 * 1024;
        const int QueueSize = 100;
        public static Action<TraceKey> EnterHook;
        public static Action<TraceKey> LeaveHook;
        static SpinLock _lock = new SpinLock();
        static int _ringTail;
        static int _ringHead;
        static FileStream _file;
        static long _epoch;
        static long _tickRation;
        static long _lastDuration;
        public static double AverageDuration;
        public static double MaximumDuration;
        public static double FrameDuration;
        static TraceRecord[] _ringBuffer;
        static byte[] _writeBuffer;

        [Conditional("TRACE")]
        public static void CalcInterval() {
            var now = Stopwatch.GetTimestamp();
            var duration = now - _lastDuration;
            _lastDuration = now;
            Averages.Enqueue(duration);
            if (Averages.Count > QueueSize)
                Averages.Dequeue();

            var sum = 0.0;
            var max = 0.0;
            foreach (var entry in Averages) {
                if (entry > max)
                    max = entry;
                sum += entry;
            }

            AverageDuration = (sum / Averages.Count) / Stopwatch.Frequency;
            MaximumDuration = max / Stopwatch.Frequency;
            FrameDuration = (double)duration / Stopwatch.Frequency;
        }

        [Conditional("TRACE")]
        public static void Start() {
            var lockTaken = false;
            _lock.Enter(ref lockTaken);
            if (_file != null) {
                _lock.Exit();
                return;
            }
            _ringHead = 0;
            _ringTail = 0;
            _epoch = Stopwatch.GetTimestamp();
            _tickRation = 16777216000000L / Stopwatch.Frequency;
            _ringBuffer = new TraceRecord[RingSize];
            _writeBuffer = new byte[WriteBufferSize];
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
            _ringBuffer = null;
            _writeBuffer = null;
            _lock.Exit();
        }

        [Conditional("TRACE")]
        [TargetedPatchingOptOut("")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Enter(TraceKey trace) {
            if (EnterHook != null)
                EnterHook(trace);
            trace.EnterTimestamp = Stopwatch.GetTimestamp();
            if (_file == null)
                return;
            TraceRecord traceRecord;
            traceRecord.Thread = Thread.CurrentThread.ManagedThreadId;
            traceRecord.Timestamp = (((Stopwatch.GetTimestamp() - _epoch) * _tickRation) >> 24);
            traceRecord.Scope = (trace.Id << 1) | 1;
            var lockTaken = false;
            _lock.Enter(ref lockTaken);
            _ringBuffer[_ringHead++] = traceRecord;
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
        [TargetedPatchingOptOut("")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Leave(TraceKey trace) {
            if (LeaveHook != null)
                LeaveHook(trace);
            trace.LiveDuration += Stopwatch.GetTimestamp() - trace.EnterTimestamp;
            if (_file == null)
                return;
            TraceRecord traceRecord;
            traceRecord.Thread = Thread.CurrentThread.ManagedThreadId;
            traceRecord.Timestamp = (((Stopwatch.GetTimestamp() - _epoch) * _tickRation) >> 24);
            traceRecord.Scope = (trace.Id << 1) | 0;
            var lockTaken = false;
            _lock.Enter(ref lockTaken);
            _ringBuffer[_ringHead++] = traceRecord;
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
        [TargetedPatchingOptOut("")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Enter(TraceKey trace, int threadId) {
            if (EnterHook != null)
                EnterHook(trace);
            trace.EnterTimestamp = Stopwatch.GetTimestamp();
            if (_file == null)
                return;
            TraceRecord traceRecord;
            traceRecord.Thread = threadId;
            traceRecord.Timestamp = (uint)(((Stopwatch.GetTimestamp() - _epoch) * _tickRation) >> 24);
            traceRecord.Scope = (trace.Id << 1) | 1;
            var lockTaken = false;
            _lock.Enter(ref lockTaken);
            _ringBuffer[_ringHead++] = traceRecord;
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
        [TargetedPatchingOptOut("")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Leave(TraceKey trace, int threadId) {
            if (LeaveHook != null)
                LeaveHook(trace);
            trace.LiveDuration += Stopwatch.GetTimestamp() - trace.EnterTimestamp;
            if (_file == null)
                return;
            TraceRecord traceRecord;
            traceRecord.Thread = threadId;
            traceRecord.Timestamp = (uint)(((Stopwatch.GetTimestamp() - _epoch) * _tickRation) >> 24);
            traceRecord.Scope = (trace.Id << 1) | 0;
            var lockTaken = false;
            _lock.Enter(ref lockTaken);
            _ringBuffer[_ringHead++] = traceRecord;
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
                        if (length > _writeBuffer.Length / RecordSize)
                            length = _writeBuffer.Length / RecordSize;
                        var bytes = length * RecordSize;
                        {
                            fixed (TraceRecord* from = &_ringBuffer[_ringTail])
                            fixed (byte* to = &_writeBuffer[0]) {
                                UnsafeNativeMethods.MoveMemory(to, from, bytes);
                            }
                        }
                        _file.Write(_writeBuffer, 0, bytes);
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

            var indexes = new int[entries.Length];
            for (var i = 0; i < entries.Length; ++i)
                indexes[i] = i;

            Array.Sort(indexes, (a, b) => {
                var ae = entries[a];
                var be = entries[b];
                var c = ae.Timestamp.CompareTo(be.Timestamp);
                if (c != 0)
                    return c;
                return a.CompareTo(b);
            });

            var temp = new TraceRecord[entries.Length];
            for (var i = 0; i < entries.Length; ++i)
                temp[i] = entries[indexes[i]];

            entries = temp;

            prevTimestamp = 0;
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
            public long Timestamp;
            public TraceKey Trace;
        }

        [StructLayout(LayoutKind.Explicit, Size = 16, Pack = 1)]
        struct TraceRecord {
            [FieldOffset(0)] public long Timestamp;
            [FieldOffset(4)] public int Thread;
            [FieldOffset(8)] public int Scope;
        }

        sealed class UnsafeNativeMethods {
            [DllImport("Kernel32.dll", EntryPoint = "RtlMoveMemory", SetLastError = false)]
            internal static extern unsafe void MoveMemory(void* dest, void* src, int size);
        }

        static readonly Queue<long> Averages = new Queue<long>(QueueSize);
    }
}
