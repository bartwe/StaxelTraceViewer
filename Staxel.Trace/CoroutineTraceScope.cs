using System.Collections.Generic;

namespace Staxel.Trace {
    public struct CoroutineTraceScope {
        public TraceKey Key;

        public static IEnumerable<T> Trace<T>(TraceKey key, IEnumerable<T> coroutine) {
            IEnumerator<T> routine;
            using (new TraceScope(key)) {
                routine = coroutine.GetEnumerator();
            }
            while (true) {
                bool moveNext;
                T current;
                using (new TraceScope(key)) {
                    moveNext = routine.MoveNext();
                    current = routine.Current;
                }
                if (!moveNext)
                    break;
                yield return current;
            }
        }
    }
}
