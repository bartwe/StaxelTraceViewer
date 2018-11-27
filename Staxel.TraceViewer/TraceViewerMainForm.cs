﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Staxel.Trace;
using Staxel.TraceViewer.Properties;

namespace Staxel.TraceViewer {
    public partial class TraceViewerMainForm : Form {
        Bar[] _bars;
        TraceRecorder.TraceEvent[] _entries;
        TraceKey[] _traceMap;
        Bitmap _newScreenBuffer;
        Bitmap _screenBuffer;
        bool _updatePending;
        bool _updateRequested;
        bool _filter;
        int _contextCount;

        public TraceViewerMainForm() {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.DoubleBuffer, true);
            InitializeComponent();
            MouseWheel += TraceViewerMainForm_MouseWheel;
        }

        void TraceViewerMainForm_Load(object sender, EventArgs e) {
            try {
                if (!string.IsNullOrEmpty(Settings.Default.FileDialogPath)) {
                    var dir = new DirectoryInfo(Path.GetFullPath(Settings.Default.FileDialogPath));
                    if (dir.Exists)
                        StaxelTraceOpenFileDialog.InitialDirectory = Settings.Default.FileDialogPath;
                }
            }
            catch (Exception ex) {
                Console.WriteLine(ex);
            }
            if (StaxelTraceOpenFileDialog.ShowDialog() == DialogResult.OK) {
                Settings.Default.FileDialogPath = Path.GetDirectoryName(Path.GetFullPath(StaxelTraceOpenFileDialog.FileName));
                Settings.Default.Save();
                LoadFromFile(StaxelTraceOpenFileDialog.FileName);
                BringToFront();
            }
            else
                Close();
        }

        void LoadFromFile(string file) {
            TraceRecorder.Load(file, TraceKeys.All(), ref _entries, ref _traceMap);
            _bars = new Bar[0];

            if (_entries.Length > 0) {
                var epoch = _entries[0].Timestamp;
                OffsetHSB.Minimum = (int)Math.Max(0, Math.Min(int.MaxValue, _entries[0].Timestamp - epoch));
                OffsetHSB.Value = OffsetHSB.Minimum;
                OffsetHSB.Maximum = (int)Math.Max(OffsetHSB.Minimum, Math.Min(int.MaxValue, _entries[_entries.Length - 1].Timestamp - epoch));

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

                var barsCount = 0;
                for (var i = 0; i < _entries.Length; ++i) {
                    if (_entries[i].Enter)
                        barsCount++;
                }
                _bars = new Bar[barsCount];
                barsCount = 0;
                for (var i = 0; i < _entries.Length; ++i) {
                    var entry = _entries[i];
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
                            if (start.TraceId == entry.TraceId)
                                break; // unwind over lost/missing pops
                        }
                        Bar bar;
                        bar.Start = start.Timestamp - epoch;
                        bar.End = entry.Timestamp - epoch;
                        if (bar.Start == bar.End)
                            bar.End = 1 + bar.End;
                        bar.TraceId = start.TraceId;
                        bar.Row = context.Stack.Count;
                        bar.ContextOffset = context.Offset;
                        bar.LateFrame = false;
                        bar.HorribleFrame = false;
                        _bars[barsCount++] = bar;
                    }
                    skip_entry:
                    {}
                }
                _contextCount = contexts.Count;
            }
            if (_contextCount < 1)
                _contextCount = 1;

            var lateFrame = false;
            var horribleFrame = false;
            for (var i = _bars.Length - 1; i >= 0; i--) {
                var bar = _bars[i];
                if (bar.Row == 0) {
                    lateFrame = (bar.End - bar.Start) > 16000;
                    horribleFrame = (bar.End - bar.Start) > 100000;
                }
                bar.LateFrame = lateFrame;
                bar.HorribleFrame = horribleFrame;
                _bars[i] = bar;
            }

            var highId = TraceKeys.All().Aggregate((a, b) => (a.Id > b.Id) ? a : b).Id;

            var sums = new Sum[highId + 1];

            foreach (var key in TraceKeys.All()) {
                var sum = new Sum();
                sum.Key = key;
                sums[key.Id] = sum;
            }
            if (sums[0] == null)
                sums[0] = new Sum();
            for (var i = 0; i < sums.Length; ++i)
                if (sums[i] == null)
                    sums[i] = sums[0];


            var _innerDuration = new Counter[256];
            for (var i = 0; i < _innerDuration.Length; ++i)
                _innerDuration[i] = new Counter();
            for (var i = 0; i < _bars.Length; i++) {
                var bar = _bars[i];

                var innerDuration = _innerDuration[bar.Row].Value;

                var duration = bar.End - bar.Start;
                var sum = sums[bar.TraceId];
                sum.Inclusive += duration;
                sum.Exclusive += duration - innerDuration;
                sum.Calls++;

                if (bar.LateFrame) {
                    sum.InclusiveLate += duration;
                    sum.ExclusiveLate += duration - innerDuration;
                    sum.CallsLate++;
                }

                if (bar.HorribleFrame) {
                    sum.InclusiveHorrible += duration;
                    sum.ExclusiveHorrible += duration - innerDuration;
                    sum.CallsHorrible++;
                }

                for (var j = 0; j < bar.Row; ++j)
                    _innerDuration[j].Value += duration;
                for (var j = bar.Row; j < _innerDuration.Length; ++j)
                    _innerDuration[j].Value = 0;
            }

            var filteredSums = sums.Where(x => x != null).ToArray();

            Console.WriteLine();
            Console.WriteLine("Inclusive: ");
            foreach (var top10 in filteredSums.Sorted((a, b) => -a.Inclusive.CompareTo(b.Inclusive)).Take(10))
                Console.WriteLine(top10.Key.Code.PadRight(68) + ": " + top10.Inclusive.ToString(CultureInfo.InvariantCulture).PadRight(12) + " \\ " + top10.Calls);
            Console.WriteLine();
            Console.WriteLine("Exclusive: ");
            foreach (var top10 in filteredSums.Sorted((a, b) => -a.Exclusive.CompareTo(b.Exclusive)).Take(10))
                Console.WriteLine(top10.Key.Code.PadRight(68) + ": " + top10.Exclusive.ToString(CultureInfo.InvariantCulture).PadRight(12) + " \\ " + top10.Calls);

            Console.WriteLine();
            Console.WriteLine("InclusiveLate: ");
            foreach (var top10 in filteredSums.Sorted((a, b) => -a.InclusiveLate.CompareTo(b.InclusiveLate)).Take(10))
                Console.WriteLine(top10.Key.Code.PadRight(68) + ": " + top10.InclusiveLate.ToString(CultureInfo.InvariantCulture).PadRight(12) + " \\ " + top10.CallsLate);
            Console.WriteLine();
            Console.WriteLine("ExclusiveLate: ");
            foreach (var top10 in filteredSums.Sorted((a, b) => -a.ExclusiveLate.CompareTo(b.ExclusiveLate)).Take(10))
                Console.WriteLine(top10.Key.Code.PadRight(68) + ": " + top10.ExclusiveLate.ToString(CultureInfo.InvariantCulture).PadRight(12) + " \\ " + top10.CallsLate);


            Console.WriteLine();
            Console.WriteLine("InclusiveHorrible: ");
            foreach (var top10 in filteredSums.Sorted((a, b) => -a.InclusiveHorrible.CompareTo(b.InclusiveHorrible)).Take(10))
                Console.WriteLine(top10.Key.Code.PadRight(68) + ": " + top10.InclusiveHorrible.ToString(CultureInfo.InvariantCulture).PadRight(12) + " \\ " + top10.CallsHorrible);
            Console.WriteLine();
            Console.WriteLine("ExclusiveHorrible: ");
            foreach (var top10 in filteredSums.Sorted((a, b) => -a.ExclusiveHorrible.CompareTo(b.ExclusiveHorrible)).Take(10))
                Console.WriteLine(top10.Key.Code.PadRight(68) + ": " + top10.ExclusiveHorrible.ToString(CultureInfo.InvariantCulture).PadRight(12) + " \\ " + top10.CallsHorrible);

            UpdateBitmap();
        }

        unsafe void UpdateBitmap() {
            if (_updatePending) {
                _updateRequested = true;
                return;
            }
            if (_bars == null)
                return;
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
            var contextScale = Height / (float)_contextCount;

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
                                    if ((px >= 0) && (px < 10000))
                                        g.DrawLine(pen, px, 0, px, _newScreenBuffer.Height);
                                }
                            }
                        });
                    const double fps = 60;
                    const double fpsInterval = 1000000 / fps;
                    const double milliSecondInterval = 1000000 / 1000;
                    const double secondInterval = 1000000;
                    const double minuteInterval = 1000000 * 60;

                    renderLines(fpsInterval, Pens.Black);
                    renderLines(milliSecondInterval, Pens.White);
                    renderLines(secondInterval, Pens.Yellow);
                    renderLines(minuteInterval, Pens.GreenYellow);

                    var blackBrush = new SolidBrush(Color.Black);

                    const int steps = 20;
                    for (var i = 0; i < steps; ++i) {
                        var px = (_newScreenBuffer.Width * i / steps);
                        var ts = (int)(px / xScale + epoch);
                        g.DrawString(ts.ToString(CultureInfo.InvariantCulture), DefaultFont, blackBrush, px, 0.0f);
                    }
                    var _barsLength = _bars.Length;
                    fixed (Bar* barp = _bars)
                        for (var i = 0; i < _barsLength; ++i) {
                            var bar = &barp[i];
                            if ((bar->End < minClamp) || (bar->Start > maxClamp))
                                continue;
                            if (_filter && !bar->LateFrame)
                                continue;

                            var x = (bar->Start - epoch) * xScale;
                            var ex = (bar->End - epoch) * xScale;
                            var width = ex - x;
                            if (width < 0.01)
                                continue;

                            var y = topOffset + bar->Row * stackScale * yScale + bar->ContextOffset * contextScale * yScale;
                            if (width < 1.0)
                                width = 1.0f;
                            const float height = stackScale * yScale;
                            var trace = _traceMap[bar->TraceId];
                            g.FillRectangle(new SolidBrush(trace.Color), x, y, width, height);
                            if (width > 50)
                                g.DrawString(trace.Code, DefaultFont, blackBrush, x, y);
                        }
                }
                UpdateBitmapCompleted();
            };

            bgWorker.RunWorkerAsync();
        }

        void UpdateBitmapCompleted() {
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

        void TraceViewerMainForm_Paint(object sender, PaintEventArgs e) {
            if (_screenBuffer != null)
                e.Graphics.DrawImage(_screenBuffer, 0, 40);
        }

        void ZoomHSB_Scroll(object sender, ScrollEventArgs e) {
            UpdateBitmap();
        }

        void OffsetHSB_Scroll(object sender, ScrollEventArgs e) {
            UpdateBitmap();
        }

        void TraceViewerMainForm_SizeChanged(object sender, EventArgs e) {
            if (_screenBuffer != null) {
                _screenBuffer.Dispose();
                _screenBuffer = null;
            }
            UpdateBitmap();
        }

        void TraceViewerMainForm_MouseWheel(object sender, MouseEventArgs e) {
            var amount = e.Delta / 120;
            if ((ModifierKeys & Keys.Shift) != 0) {
                ZoomHSB.Value = Math.Min(Math.Max(ZoomHSB.Value + ZoomHSB.LargeChange * amount, ZoomHSB.Minimum), ZoomHSB.Maximum);
            }
            else {
                OffsetHSB.Value = Math.Min(Math.Max(OffsetHSB.Value + OffsetHSB.LargeChange * amount, OffsetHSB.Minimum), OffsetHSB.Maximum);
            }
            UpdateBitmap();
        }

        void checkBox1_CheckedChanged(object sender, EventArgs e) {
            _filter = checkBox1.Checked;
            UpdateBitmap();
        }

        sealed class Counter {
            public long Value;
        }

        sealed class Sum {
            public TraceKey Key;
            public long Inclusive;
            public long Exclusive;
            public long Calls;
            public long InclusiveLate;
            public long ExclusiveLate;
            public long CallsLate;
            public long InclusiveHorrible;
            public long ExclusiveHorrible;
            public long CallsHorrible;
        }

        struct Bar {
            public long End;
            public long Start;
            public int TraceId;
            public int ContextOffset;
            public int Row;
            public bool LateFrame;
            public bool HorribleFrame;
        }

        sealed class Context {
            public readonly Stack<TraceRecorder.TraceEvent> Stack = new Stack<TraceRecorder.TraceEvent>();
            public int Offset;
        }
    }
}
