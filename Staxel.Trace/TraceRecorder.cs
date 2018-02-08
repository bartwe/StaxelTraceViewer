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
        const int RingFlushSize = 524288;
        const int RingSize = RingFlushSize * 2;
        const int WriteBufferSize = 64 * 1024;
        static SpinLock _lock = new SpinLock(Debugger.IsAttached);
        static int _ringTail;
        static int _ringHead;
        static FileStream _file;
        static long _epoch;
        static long _tickRation;
        static TraceRecord[] _ringBuffer;
        static byte[] _writeBuffer;

        [Conditional("TRACE")]
        public static void Start() {
            var lockTaken = false;
            _lock.Enter(ref lockTaken);
            try {
                if (_file != null)
                    return;
                try {
                    if (_ringBuffer == null)
                        _ringBuffer = new TraceRecord[RingSize];
                    if (_writeBuffer == null)
                        _writeBuffer = new byte[WriteBufferSize];
                }
                catch (Exception e) {
                    Console.WriteLine("Failed to allocate buffers for trace recording");
                    Console.WriteLine(e);
                    _ringBuffer = null;
                    _writeBuffer = null;
                    return;
                }
                _ringHead = 0;
                _ringTail = 0;
                _epoch = Stopwatch.GetTimestamp();
                _tickRation = 16777216000000L / Stopwatch.Frequency;
                _file = new FileStream(DateTime.Now.ToString("yyyyMMddHHmmss", CultureInfo.InvariantCulture) + ".staxeltrace", FileMode.CreateNew);
            }
            finally {
                _lock.Exit();
            }
        }

        [Conditional("TRACE")]
        public static void Stop(bool releaseBuffers = true) {
            Flush(true);
            var lockTaken = false;
            _lock.Enter(ref lockTaken);
            try {
                if (_file != null) {
                    _file.Close();
                    _file = null;
                }
                if (releaseBuffers) {
                    _ringBuffer = null;
                    _writeBuffer = null;
                }
            }
            finally {
                _lock.Exit();
            }
        }

        [Conditional("TRACE")]
        [TargetedPatchingOptOut("")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Enter(TraceKey trace) {
            if (_file == null)
                return;
            TraceRecord traceRecord;
            traceRecord.Thread = Thread.CurrentThread.ManagedThreadId;
            traceRecord.Timestamp = (((Stopwatch.GetTimestamp() - _epoch) * _tickRation) >> 24);
            traceRecord.Scope = (trace.Id << 1) | 1;
            var lockTaken = false;
            _lock.Enter(ref lockTaken);
            if (_file != null) {
                _ringBuffer[_ringHead++] = traceRecord;
                if (_ringHead == RingSize)
                    _ringHead = 0;
                if (_ringTail == _ringHead) {
                    _ringTail++;
                    if (_ringTail == RingSize)
                        _ringTail = 0;
                }
            }
            _lock.Exit();
        }

        [Conditional("TRACE")]
        [TargetedPatchingOptOut("")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Leave(TraceKey trace) {
            if (_file == null)
                return;
            TraceRecord traceRecord;
            traceRecord.Thread = Thread.CurrentThread.ManagedThreadId;
            traceRecord.Timestamp = (((Stopwatch.GetTimestamp() - _epoch) * _tickRation) >> 24);
            traceRecord.Scope = (trace.Id << 1) | 0;
            var lockTaken = false;
            _lock.Enter(ref lockTaken);
            if (_file != null) {
                _ringBuffer[_ringHead++] = traceRecord;
                if (_ringHead == RingSize)
                    _ringHead = 0;
                if (_ringTail == _ringHead) {
                    _ringTail++;
                    if (_ringTail == RingSize)
                        _ringTail = 0;
                }
            }
            _lock.Exit();
        }

        [Conditional("TRACE")]
        [TargetedPatchingOptOut("")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Enter(TraceKey trace, int threadId) {
            if (_file == null)
                return;
            TraceRecord traceRecord;
            traceRecord.Thread = threadId;
            traceRecord.Timestamp = (uint)(((Stopwatch.GetTimestamp() - _epoch) * _tickRation) >> 24);
            traceRecord.Scope = (trace.Id << 1) | 1;
            var lockTaken = false;
            _lock.Enter(ref lockTaken);
            if (_file != null) {
                _ringBuffer[_ringHead++] = traceRecord;
                if (_ringHead == RingSize)
                    _ringHead = 0;
                if (_ringTail == _ringHead) {
                    _ringTail++;
                    if (_ringTail == RingSize)
                        _ringTail = 0;
                }
            }
            _lock.Exit();
        }

        [Conditional("TRACE")]
        [TargetedPatchingOptOut("")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Leave(TraceKey trace, int threadId) {
            if (_file == null)
                return;
            TraceRecord traceRecord;
            traceRecord.Thread = threadId;
            traceRecord.Timestamp = (uint)(((Stopwatch.GetTimestamp() - _epoch) * _tickRation) >> 24);
            traceRecord.Scope = (trace.Id << 1) | 0;
            var lockTaken = false;
            _lock.Enter(ref lockTaken);
            if (_file != null) {
                _ringBuffer[_ringHead++] = traceRecord;
                if (_ringHead == RingSize)
                    _ringHead = 0;
                if (_ringTail == _ringHead) {
                    _ringTail++;
                    if (_ringTail == RingSize)
                        _ringTail = 0;
                }
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
                if (_file != null) {
                    var entries = _ringHead - _ringTail;
                    if (entries < 0)
                        entries += RingSize;
                    var flush = hard | (entries >= RingFlushSize);
                    if (flush) {
                        Console.WriteLine("Flush trace");
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
            [FieldOffset(8)] public int Thread;
            [FieldOffset(12)] public int Scope;
        }

        static class UnsafeNativeMethods {
            [DllImport("Kernel32.dll", EntryPoint = "RtlMoveMemory", SetLastError = false)]
            internal static extern unsafe void MoveMemory(void* dest, void* src, int size);
        }
    }
}
