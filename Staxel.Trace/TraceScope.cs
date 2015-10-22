using System;
using System.Diagnostics;
using System.Runtime;
using System.Runtime.CompilerServices;

namespace Staxel.Trace {
    public struct TraceScope : IDisposable {
        public TraceKey Key;

        [TargetedPatchingOptOut("")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TraceScope(TraceKey key) {
            Key = key;
            TraceRecorder.Enter(Key);
        }

        [TargetedPatchingOptOut("")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose() {
            TraceRecorder.Leave(Key);
        }
    }
}
