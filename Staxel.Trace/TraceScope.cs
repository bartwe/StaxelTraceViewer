using System;

namespace Staxel.Trace{
    public struct TraceScope : IDisposable{
        public TraceKey Key;
        private readonly Action<TraceKey> _leave;

        public TraceScope(TraceKey key, Action<TraceKey> enter = null, Action<TraceKey> leave = null){
            Key = key;
            _leave = leave;

            if(enter != null){
                enter(Key);
            }
            TraceRecorder.Enter(Key);
        }

        public void Dispose(){
            if(_leave != null){
                _leave(Key);
            }
            TraceRecorder.Leave(Key);
        }
    }
}