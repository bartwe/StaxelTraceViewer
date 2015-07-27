using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;

namespace Staxel.Trace {
    public class TraceKey{
        public TraceKey(string code, int id, Color color) {
            Code = code;
            Id = id;
            Color = color;
        }

        public TraceKey(Color color) {
            Color = color;
        }


        public string Code { get; set; }
        public int Id { get; set; }
        public Color Color { get; private set; }
        public long LiveDuration { get; set; }
    }
}
