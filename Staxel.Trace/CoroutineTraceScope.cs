using System;
using System.Runtime;
using System.Runtime.CompilerServices;

namespace Staxel.Trace {
    public struct CoroutineTraceScope : IDisposable {
        public TraceKey Key;
        public int CoroutineId;

        [TargetedPatchingOptOut("")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public CoroutineTraceScope(TraceKey key, TraceKey coroutineId) {
            Key = key;
            CoroutineId = coroutineId.Id;
            TraceRecorder.Enter(Key, CoroutineId);
        }

        [TargetedPatchingOptOut("")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose() {
            TraceRecorder.Leave(Key, CoroutineId);
        }
    }
}