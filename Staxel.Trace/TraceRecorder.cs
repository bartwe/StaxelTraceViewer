﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;

namespace Staxel.Trace {
    public class TraceRecorder {
        [StructLayout(LayoutKind.Explicit, Size = 12, Pack = 1)]
        private struct TraceRecord {
            [FieldOffset(0)]
            public int Timestamp;
            [FieldOffset(4)]
            public int Thread;
            [FieldOffset(8)]
            public int Scope;
        }

        private const int RecordSize = 12; // must match struct Entry's size
        private const int RingSize = 10000000;
        private const int RingFlushSize = 1000000;
        private const int WriteBufferSize = 64 * 1024;

        private static readonly TraceRecord[] RingBuffer = new TraceRecord[RingSize];
        private static readonly object Locker = new object();
        private static readonly byte[] WriteBuffer = new byte[WriteBufferSize];

        private static int _ringTail;
        private static int _ringHead;
        private static Stopwatch _stopwatch;
        private static FileStream _file;

        public static void Start() {
            lock (Locker) {
                _stopwatch = new Stopwatch();
                _stopwatch.Start();
                _file = new FileStream(DateTime.Now.ToString("yyyyMMddHHmmss", CultureInfo.InvariantCulture) + ".staxeltrace", FileMode.CreateNew);
            }
        }

        public static void Stop() {
            lock (Locker) {
                if (_file == null)
                    return;
                Flush(true);
                _file.Close();
                _file = null;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Enter(TraceKey trace) {
            if (_file == null)
                return;
            TraceRecord traceRecord;
            traceRecord.Thread = Thread.CurrentThread.ManagedThreadId;
            traceRecord.Timestamp = (int)((_stopwatch.ElapsedTicks * 1000000L) / Stopwatch.Frequency);
            traceRecord.Scope = (trace.Id << 1) | 1;
            lock (Locker) {
                RingBuffer[_ringHead++] = traceRecord;
                if (_ringHead == RingSize)
                    _ringHead = 0;
                if (_ringTail == _ringHead) {
                    _ringTail++;
                    if (_ringTail > RingSize)
                        _ringTail = 0;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Leave(TraceKey trace) {
            if (_file == null)
                return;
            TraceRecord traceRecord;
            traceRecord.Thread = Thread.CurrentThread.ManagedThreadId;
            traceRecord.Timestamp = (int)((_stopwatch.ElapsedTicks * 1000000L) / Stopwatch.Frequency);
            traceRecord.Scope = (trace.Id << 1) | 0;
            lock (Locker) {
                RingBuffer[_ringHead++] = traceRecord;
                if (_ringHead == RingSize)
                    _ringHead = 0;
                if (_ringTail == _ringHead) {
                    _ringTail++;
                    if (_ringTail > RingSize)
                        _ringTail = 0;
                }
            }
        }

        private class UnsafeNativeMethods {
            [DllImport("Kernel32.dll", EntryPoint = "RtlMoveMemory", SetLastError = false)]
            internal static extern unsafe void MoveMemory(void* dest, void* src, int size);
        }

        public static unsafe void Flush(bool hard = false) {
            lock (Locker) {
                if (_file == null)
                    return;
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
        }

        private unsafe static TraceRecord[] LoadRaw(string file) {
            var bytes = File.ReadAllBytes(file);
            var entries = bytes.Length / RecordSize;
            var result = new TraceRecord[entries];
            fixed (byte* from = &bytes[0])
            fixed (TraceRecord* to = &result[0]) {
                UnsafeNativeMethods.MoveMemory(to, from, entries * RecordSize);
            }
            return result;
        }

        public struct TraceEvent {
            public int Timestamp;
            public int Thread;
            public TraceKey Trace;
            public bool Enter;
        }

        public static TraceEvent[] Load(string file, IEnumerable<TraceKey> keys) {
            var keysMap = new Dictionary<int, TraceKey>();
            foreach (var key in keys)
                keysMap.Add(key.Id, key);
            var entries = LoadRaw(file);
            var result = new TraceEvent[entries.Length];
            for (int i = 0; i < entries.Length; ++i) {
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
    }
}
