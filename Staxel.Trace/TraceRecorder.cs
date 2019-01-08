﻿using System;
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
        static readonly unsafe int RecordSize = sizeof(TraceRecord);
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
                _file = new FileStream(DateTime.Now.ToString("yyyyMMddHHmmss", CultureInfo.InvariantCulture) + "_" + Guid.NewGuid().ToString("N") + ".staxeltrace", FileMode.CreateNew);
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
                                fixed (TraceRecord* from = &_ringBuffer[0])
                                fixed (byte* to = &_writeBuffer[0]) {
                                    UnsafeNativeMethods.Copy(from, _ringTail, _ringBuffer.Length, sizeof(TraceRecord), to, 0, _writeBuffer.Length, 1, bytes);
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
                UnsafeNativeMethods.Copy(from, 0, bytes.Length, 1, to, 0, result.Length, sizeof(TraceRecord), entries * RecordSize);
            }
            return result;
        }

        public static unsafe void Load(string file, IEnumerable<TraceKey> keys, ref TraceEvent[] _events, ref TraceKey[] _traceMap) {
            var keysMap = new Dictionary<int, int>();
            var keysList = new List<TraceKey>();
            foreach (var key in keys) {
                keysMap.Add(key.Id, keysList.Count);
                keysList.Add(key);
            }
            var entries = LoadRaw(file);
            long prevTimestamp = 0;

            var indexes = new int[entries.Length];
            for (var i = 0; i < entries.Length; ++i)
                indexes[i] = i;

            var entries1 = entries;
            Array.Sort(indexes, (a, b) => {
                var ae = entries1[a].Timestamp;
                var be = entries1[b].Timestamp;
                if (ae < be)
                    return -1;
                if (ae > be)
                    return 1;
                if (a < b)
                    return -1;
                return a > b ? 1 : 0;
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
            var entriesMax = 2146435071 / 24;
            var entriesOffset = 0;
            if (entriesLimit > entriesMax) {
                entriesOffset = entriesLimit - entriesMax;
                entriesLimit = entriesMax;
            }

            var result = new TraceEvent[entriesLimit];
            for (var i = 0; i < entriesLimit; ++i) {
                var entry = entries[i + entriesOffset];
                TraceEvent e;
                e.Timestamp = entry.Timestamp;
                e.Thread = entry.Thread;
                e.Enter = (entry.Scope & 1) == 1;
                e.TraceId = keysMap[entry.Scope >> 1];
                result[i] = e;
            }
            _events = result;
            _traceMap = keysList.ToArray();
        }

        public struct TraceEvent {
            public long Timestamp;
            public int TraceId;
            public int Thread;
            public bool Enter;
        }

        [StructLayout(LayoutKind.Explicit, Size = 16, Pack = 1)]
        struct TraceRecord {
            [FieldOffset(0)] public long Timestamp;
            [FieldOffset(8)] public int Thread;
            [FieldOffset(12)] public int Scope;
        }

        static class UnsafeNativeMethods {
            [DllImport("Kernel32.dll", EntryPoint = "RtlMoveMemory", SetLastError = false)]
            static extern unsafe void MoveMemory(void* dest, void* src, int size);

            public static unsafe void Copy(void* from, int fromOffset, int fromTotalLength, int fromElementSize, void* target, int targetOffset, int targetTotalLength, int targetElementSize, int bytes) {
                if (bytes == 0)
                    return;
                if (bytes < 0)
                    throw new ArgumentOutOfRangeException("bytes");
                if (from == null)
                    throw new ArgumentNullException("from");
                if (target == null)
                    throw new ArgumentNullException("to");
                if (fromOffset < 0)
                    throw new ArgumentOutOfRangeException("fromOffset");
                if (targetOffset < 0)
                    throw new ArgumentOutOfRangeException("targetOffset");
                if (fromTotalLength < 0)
                    throw new ArgumentOutOfRangeException("fromTotalLength");
                if (targetTotalLength < 0)
                    throw new ArgumentOutOfRangeException("targetTotalLength");
                if (fromOffset >= fromTotalLength)
                    throw new ArgumentOutOfRangeException("fromOffset");
                if (targetOffset >= targetTotalLength)
                    throw new ArgumentOutOfRangeException("targetOffset");
                if (fromElementSize <= 0)
                    throw new ArgumentOutOfRangeException("fromElementSize");
                if (targetElementSize <= 0)
                    throw new ArgumentOutOfRangeException("targetElementSize");

                fromOffset *= fromElementSize;
                fromTotalLength *= fromElementSize;

                targetOffset *= targetElementSize;
                targetTotalLength *= targetElementSize;

                if (fromTotalLength - fromOffset < bytes)
                    throw new ArgumentOutOfRangeException("bytes");

                if (targetTotalLength - targetOffset < bytes)
                    throw new ArgumentOutOfRangeException("bytes");

                MoveMemory((byte*)target + targetOffset, (byte*)from + fromOffset, bytes);
            }
        }
    }
}
