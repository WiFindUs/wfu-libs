using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using WiFindUs.Extensions;

namespace WiFindUs.Forms
{
    public partial class SplashForm : BaseForm
    {
        private Image logo = null;
        private Rectangle activeArea = Rectangle.Empty;
        private string statusString = "";
        private List<Func<bool>> tasks;

        /////////////////////////////////////////////////////////////////////
        // PROPERTIES
        /////////////////////////////////////////////////////////////////////

        public string Status
        {
            get
            {
                return statusString;
            }

            set
            {
                string val = (value ?? "").Trim();
                if (val.CompareTo(statusString) == 0)
                    return;
                statusString = val;
                this.RefreshThreadSafe();
            }

        }
        
        /////////////////////////////////////////////////////////////////////
        // CONSTRUCTORS
        /////////////////////////////////////////////////////////////////////

        public SplashForm(List<Func<bool>> tasks)
        {
            InitializeComponent();
            if (IsDesignMode)
                return;
            if (tasks == null)
                throw new ArgumentNullException("tasks");
            this.tasks = tasks;
            TopMost = true;
            logo = WFUApplication.Images.Resource("wfu_logo_small");
            RecalculateActiveArea();

            loadingWorker.DoWork += LoadingThread;
            loadingWorker.ProgressChanged += LoadingProgress;
            loadingWorker.RunWorkerCompleted += LoadingCompeted;
        }

        public SplashForm() : this(null) { }

        /////////////////////////////////////////////////////////////////////
        // PROTECTED METHODS
        /////////////////////////////////////////////////////////////////////

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            if (IsDesignMode)
                return;
            RecalculateActiveArea();
            Refresh();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            if (IsDesignMode)
                return;

            if (logo != null)
                e.Graphics.DrawImage(logo, activeArea.Location);

            e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            using (Font font = new Font(Font.FontFamily, 32.0f))
            {
                e.Graphics.DrawString(
                    WFUApplication.Name,
                    font,
                    Brushes.White,
                    new Point(activeArea.Left * 2 + logo.Width, activeArea.Top),
                    StringFormat.GenericTypographic);
            }

            int top = activeArea.Top + 2*logo.Height;
            using (Font font = new Font(Font.FontFamily, 14.0f))
            {
                string text = WFUApplication.Edition + " Edition";

                SizeF sz = e.Graphics.MeasureString(
                    text,
                    font,
                    activeArea.Width,
                    StringFormat.GenericTypographic);

                e.Graphics.DrawString(
                    text,
                    font,
                    Brushes.White,
                    new Point(activeArea.Left, top),
                    StringFormat.GenericTypographic);

                top += (int)sz.Height;

                e.Graphics.DrawString(
                    "v" + WFUApplication.AssemblyVersion.ToString(),
                    font,
                    Brushes.Gray,
                    new Point(activeArea.Left, top),
                    StringFormat.GenericTypographic);
#if DEBUG
                top += (int)sz.Height;

                e.Graphics.DrawString(
                    "[Debug Compilation]",
                    font,
                    Brushes.Gray,
                    new Point(activeArea.Left, top),
                    StringFormat.GenericTypographic);
#endif
            }

                         
            if (statusString.Length > 0)
            {
                using (Font font = new Font(Font.FontFamily, 11.0f))
                { 
                    SizeF sz = e.Graphics.MeasureString(
                        statusString,
                        font,
                        activeArea.Width,
                        StringFormat.GenericTypographic);

                    e.Graphics.DrawString(
                        statusString,
                        font,
                        Brushes.Gray,
                        new PointF(activeArea.Left, progressBar.Top-sz.Height*1.5f),
                        StringFormat.GenericTypographic);
                }
            }
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            if (IsDesignMode || loadingWorker.IsBusy)
                return;
            loadingWorker.RunWorkerAsync(tasks);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (loadingWorker.IsBusy && e.CloseReason == CloseReason.UserClosing)
                e.Cancel = true;
            else
                base.OnFormClosing(e);
        }

        /////////////////////////////////////////////////////////////////////
        // PRIVATE METHODS
        /////////////////////////////////////////////////////////////////////

        private void LoadingThread(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = (BackgroundWorker)sender;
            List<Func<bool>> tasks = (List<Func<bool>>)e.Argument;

            for (int i = 0; i < tasks.Count; i++)
            {
                if (!tasks[i]())
                {
                    e.Cancel = true;
                    break;
                }
                worker.ReportProgress(MathHelper.WholePercentage(i + 1, tasks.Count));
#if DEBUG
                Thread.Sleep(250);
#endif
            }
        }

        private void LoadingProgress(object sender, ProgressChangedEventArgs e)
        {
            progressBar.Value = e.ProgressPercentage;
            Refresh();
        }

        private void LoadingCompeted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null || e.Cancelled)
                WFUApplication.MainForm.Close();
            else
            {
                WFUApplication.SplashLoadingFinished = true;
                Close();
            }
        }

        private void RecalculateActiveArea()
        {
            int top = ClientRectangle.Height - progressBar.Bottom;
            activeArea = new Rectangle(progressBar.Left,
                top,
                ClientRectangle.Width - (progressBar.Left * 2),
                progressBar.Top - top);
        }
    }
}
