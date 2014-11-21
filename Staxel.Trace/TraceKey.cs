﻿using System.Drawing;

namespace Staxel.Trace {
    public class TraceKey {
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
    }
}
