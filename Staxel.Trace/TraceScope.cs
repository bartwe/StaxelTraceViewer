using System;

namespace Staxel.Trace {
    public struct TraceScope: IDisposable {
        public TraceScope(TraceKey key) {
            Key = key;
            TraceRecorder.Enter(Key);
        }

        public TraceKey Key;

        public void Dispose() {
            TraceRecorder.Leave(Key);
        }
    }
}
