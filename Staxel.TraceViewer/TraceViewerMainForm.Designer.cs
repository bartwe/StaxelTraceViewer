namespace Staxel.TraceViewer {
    partial class TraceViewerMainForm {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.StaxelTraceOpenFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.ZoomHSB = new System.Windows.Forms.HScrollBar();
            this.OffsetHSB = new System.Windows.Forms.HScrollBar();
            this.SuspendLayout();
            // 
            // StaxelTraceOpenFileDialog
            // 
            this.StaxelTraceOpenFileDialog.DefaultExt = "staxeltrace";
            this.StaxelTraceOpenFileDialog.Filter = "Staxel Trace|*.staxeltrace";
            this.StaxelTraceOpenFileDialog.InitialDirectory = "C:\\hg\\Staxel\\Staxel.Client\\bin\\Debug";
            // 
            // ZoomHSB
            // 
            this.ZoomHSB.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ZoomHSB.LargeChange = 1;
            this.ZoomHSB.Location = new System.Drawing.Point(9, 0);
            this.ZoomHSB.Maximum = 150;
            this.ZoomHSB.Minimum = -150;
            this.ZoomHSB.Name = "ZoomHSB";
            this.ZoomHSB.Size = new System.Drawing.Size(1479, 19);
            this.ZoomHSB.TabIndex = 0;
            this.ZoomHSB.Scroll += new System.Windows.Forms.ScrollEventHandler(this.ZoomHSB_Scroll);
            // 
            // OffsetHSB
            // 
            this.OffsetHSB.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.OffsetHSB.Location = new System.Drawing.Point(9, 19);
            this.OffsetHSB.Name = "OffsetHSB";
            this.OffsetHSB.Size = new System.Drawing.Size(1479, 19);
            this.OffsetHSB.TabIndex = 1;
            this.OffsetHSB.Scroll += new System.Windows.Forms.ScrollEventHandler(this.OffsetHSB_Scroll);
            // 
            // TraceViewerMainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1497, 853);
            this.Controls.Add(this.OffsetHSB);
            this.Controls.Add(this.ZoomHSB);
            this.Name = "TraceViewerMainForm";
            this.Text = "Staxel TraceViewer";
            this.Load += new System.EventHandler(this.TraceViewerMainForm_Load);
            this.SizeChanged += new System.EventHandler(this.TraceViewerMainForm_SizeChanged);
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.TraceViewerMainForm_Paint);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.OpenFileDialog StaxelTraceOpenFileDialog;
        private System.Windows.Forms.HScrollBar ZoomHSB;
        private System.Windows.Forms.HScrollBar OffsetHSB;
    }
}

