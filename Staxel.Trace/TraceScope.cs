using System;

namespace Staxel.Trace {
    public struct TraceScope : IDisposable {
        public TraceKey Key;

        public TraceScope(TraceKey key) {
            Key = key;
            TraceRecorder.Enter(Key);
        }

        public void Dispose() {
            TraceRecorder.Leave(Key);
        }
    }
}
