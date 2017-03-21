using System;
using System.Collections.Generic;
using System.Linq;

namespace Staxel.TraceViewer {
    public static class Helper {
        public static IEnumerable<T> Sorted<T>(this IEnumerable<T> self, Comparison<T> comparator) {
            var temp = self.ToList();
            temp.Sort(comparator);
            return temp;
        }
    }
}
