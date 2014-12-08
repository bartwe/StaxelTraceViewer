using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.Windows.Forms;
using Staxel.Trace;

namespace Staxel.TraceViewer {
    public partial class TraceViewerMainForm : Form {
        private Bar[] _bars;
        private TraceRecorder.TraceEvent[] _entries;
        private Bitmap _newScreenBuffer;
        private Bitmap _screenBuffer;
        private bool _updatePending;
        private bool _updateRequested;
        private bool _filter;

        public TraceViewerMainForm() {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.DoubleBuffer, true);
            InitializeComponent();
            MouseWheel += TraceViewerMainForm_MouseWheel;
        }

        private void TraceViewerMainForm_Load(object sender, EventArgs e) {
            if (StaxelTraceOpenFileDialog.ShowDialog() == DialogResult.OK) {
                LoadFromFile(StaxelTraceOpenFileDialog.FileName);
                BringToFront();
            }
            else
                Close();
        }

        private void LoadFromFile(string file) {
            _entries = TraceRecorder.Load(file, TraceKeys.All());
            var bars = new List<Bar>();

            if (_entries.Length > 0) {
                OffsetHSB.Minimum = _entries[0].Timestamp;
                OffsetHSB.Value = _entries[0].Timestamp;
                OffsetHSB.Maximum = _entries[_entries.Length - 1].Timestamp;

                var contexts = new Dictionary<int, Context>();
                var fetchContext = new Func<int, Context>(
                    threadid => {
                        Context result;
                        if (contexts.TryGetValue(threadid, out result))
                            return result;
                        result = new Context();
                        result.Offset = contexts.Count;
                        contexts.Add(threadid, result);
                        return result;
                    });

                foreach (var entry in _entries) {
                    var context = fetchContext(entry.Thread);
                    if (entry.Enter) {
                        context.Stack.Push(entry);
                    }
                    else {
                        TraceRecorder.TraceEvent start;
                        while (true) {
                            if (context.Stack.Count == 0)
                                goto skip_entry; // unbalanced stack can happen due to clamped history
                            start = context.Stack.Pop();
                            if (start.Trace == entry.Trace)
                                break; // unwind over lost/missing pops
                        }

                        Bar bar;
                        bar.Start = start.Timestamp;
                        bar.End = entry.Timestamp;
                        if (bar.Start == bar.End)
                            bar.End = 1 + bar.End;
                        bar.Trace = start.Trace;
                        bar.Row = context.Stack.Count;
                        bar.ContextOffset = context.Offset;
                        bar.Filtered = false;
                        bars.Add(bar);
                    }
                skip_entry: { }
                }
            }
            _bars = bars.ToArray();

            bool filtered = false;
            for (int i = _bars.Length - 1; i >= 0; i--) {
                var bar = _bars[i];
                if (bar.Row == 0)
                    filtered = (bar.End - bar.Start) <= 16000;
                bar.Filtered = filtered;
                _bars[i] = bar;
            }

            UpdateBitmap();
        }

        private void UpdateBitmap() {
            if (_updatePending) {
                _updateRequested = true;
                return;
            }
            _updatePending = true;

            if ((_newScreenBuffer != null) && ((_newScreenBuffer.Width != Width) || (_newScreenBuffer.Height != Height))) {
                _newScreenBuffer.Dispose();
                _newScreenBuffer = null;
            }
            if (_newScreenBuffer == null)
                _newScreenBuffer = new Bitmap(Width, Height, PixelFormat.Format24bppRgb);

            var scaling = Math.Pow(2.0, ZoomHSB.Value / 10.0);
            var xScale = 0.001f * (float)scaling;
            const float yScale = 1.0f;
            const float stackScale = 16.0f;
            const float topOffset = 32.0f;
            const float contextScale = 100.0f;

            OffsetHSB.LargeChange = (int)Math.Min(int.MaxValue, ((OffsetHSB.Maximum - OffsetHSB.Minimum) / 1000.0) / scaling);
            var epoch = (int)(OffsetHSB.Value - _newScreenBuffer.Width / xScale * 0.5);
            var minClamp = epoch;
            var maxClamp = (int)Math.Ceiling(_newScreenBuffer.Width / xScale + epoch);
            var bgWorker = new BackgroundWorker();
            bgWorker.DoWork += (sender, doWorkEventArgs) => {
                using (var g = Graphics.FromImage(_newScreenBuffer)) {
                    g.Clear(Color.LightGray);

                    var renderLines = new Action<double, Pen>(
                        (interval, pen) => {
                            var minFrame = (int)Math.Floor(minClamp / interval);
                            var maxFrame = (int)Math.Ceiling(maxClamp / interval);

                            if ((maxFrame - minFrame) < 20) {
                                for (var i = minFrame; i <= maxFrame; ++i) {
                                    var px = (float)((i * interval - epoch) * xScale);
                                    g.DrawLine(pen, px, 0, px, _newScreenBuffer.Height);
                                }
                            }
                        });
                    const double fps = 60;
                    const double fpsInterval = 1000000 / fps;
                    const double milliSecondInterval = 1000000 / 1000;
                    const double secondInterval = 1000000;
                    const double minuteInterval = 1000000 * 60;

                    renderLines(milliSecondInterval, Pens.White);
                    renderLines(secondInterval, Pens.Gray);
                    renderLines(minuteInterval, Pens.DarkGray);
                    renderLines(fpsInterval, Pens.Black);

                    const int steps = 20;
                    for (var i = 0; i < steps; ++i) {
                        var px = (_newScreenBuffer.Width * i / steps);
                        var ts = (int)(px / xScale + epoch);
                        g.DrawString(ts.ToString(CultureInfo.InvariantCulture), DefaultFont, new SolidBrush(Color.Black), px, 0.0f);
                    }
                    for (var i = 0; i < _bars.Length; ++i) {
                        var bar = _bars[i];

                        if (_filter && bar.Filtered)
                            continue;
                        if (bar.End < minClamp)
                            continue;
                        if (bar.Start > maxClamp)
                            continue;

                        var x = (bar.Start - epoch) * xScale;
                        var ex = (bar.End - epoch) * xScale;
                        var width = ex - x;
                        if (width < 0.01)
                            continue;

                        var y = topOffset + bar.Row * stackScale * yScale + bar.ContextOffset * contextScale * yScale;
                        if (width < 1.0)
                            width = 1.0f;
                        var height = stackScale * yScale;
                        g.FillRectangle(new SolidBrush(bar.Trace.Color), x, y, width, height);
                        if (width > 50)
                            g.DrawString(bar.Trace.Code, DefaultFont, new SolidBrush(Color.Black), x, y);
                    }
                }
                UpdateBitmapCompleted();
            };

            bgWorker.RunWorkerAsync();
        }

        private void UpdateBitmapCompleted() {
            if (InvokeRequired) {
                BeginInvoke(new MethodInvoker(UpdateBitmapCompleted));
            }
            else {
                _updatePending = false;
                var t = _screenBuffer;
                _screenBuffer = _newScreenBuffer;
                _newScreenBuffer = t;
                Invalidate();
                if (_updateRequested) {
                    _updateRequested = false;
                    Update();
                }
            }
        }

        private void TraceViewerMainForm_Paint(object sender, PaintEventArgs e) {
            if (_screenBuffer != null)
                e.Graphics.DrawImage(_screenBuffer, 0, 40);
        }

        private void ZoomHSB_Scroll(object sender, ScrollEventArgs e) {
            UpdateBitmap();
        }

        private void OffsetHSB_Scroll(object sender, ScrollEventArgs e) {
            UpdateBitmap();
        }

        private void TraceViewerMainForm_SizeChanged(object sender, EventArgs e) {
            if (_screenBuffer != null) {
                _screenBuffer.Dispose();
                _screenBuffer = null;
            }
            UpdateBitmap();
        }

        private void TraceViewerMainForm_MouseWheel(object sender, MouseEventArgs e) {
            var amount = e.Delta / 120;
            if ((ModifierKeys & Keys.Shift) != 0) {
                ZoomHSB.Value = Math.Min(Math.Max(ZoomHSB.Value + ZoomHSB.LargeChange * amount, ZoomHSB.Minimum), ZoomHSB.Maximum);
            }
            else {
                OffsetHSB.Value = Math.Min(Math.Max(OffsetHSB.Value + OffsetHSB.LargeChange * amount, OffsetHSB.Minimum), OffsetHSB.Maximum);
            }
            UpdateBitmap();
        }

        private struct Bar {
            public int ContextOffset;
            public int End;
            public int Row;
            public int Start;
            public TraceKey Trace;
            public bool Filtered;
        }

        private class Context {
            public readonly Stack<TraceRecorder.TraceEvent> Stack = new Stack<TraceRecorder.TraceEvent>();
            public int Offset;
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e) {
            _filter = checkBox1.Checked;
            UpdateBitmap();
        }
    }
}
