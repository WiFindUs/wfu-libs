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
using WiFindUs.Controls;
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
                return IsDesignMode ? "Loading operation status" : statusString;
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
            RecalculateActiveArea();

            if (IsDesignMode)
            {
                logo = new Bitmap(75, 97, PixelFormat.Format24bppRgb);
                using (Graphics grp = Graphics.FromImage(logo))
                    grp.FillRectangle(Brushes.White, 0, 0, logo.Width, logo.Height);
                
                return;
            }

            //set tasks
            if (tasks == null)
                throw new ArgumentNullException("tasks");
            this.tasks = tasks;
            
            //cosmetics
            logo = WFUApplication.Images.Resource("wfu_logo_small");

            //worker events
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
            RecalculateActiveArea();
            base.OnResize(e);            
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            bool design = IsDesignMode;

            //logo
            if (logo != null)
                e.Graphics.DrawImage(logo, activeArea.Location);

            if (!design)
                e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            //title
            e.Graphics.DrawString(
                design ? "Application Name" : WFUApplication.Name,
                Theme.TitleFont,
                Theme.TextLightBrush,
                new Point(activeArea.Left * 2 + logo.Width, activeArea.Top),
                StringFormat.GenericTypographic);

            //edition
            int top = activeArea.Top + 2 * logo.Height;
            string text = (design ? "Application" : WFUApplication.Edition) + " Edition";
            SizeF sz = e.Graphics.MeasureString(
                text,
                Theme.SubtitleFont,
                activeArea.Width,
                StringFormat.GenericTypographic);
            e.Graphics.DrawString(
                text,
                Theme.SubtitleFont,
                Theme.TextLightBrush,
                new Point(activeArea.Left, top),
                StringFormat.GenericTypographic);

            //version
            top += (int)sz.Height;
            e.Graphics.DrawString(
                "v" + (design ? "1.0.0.0" : WFUApplication.AssemblyVersion.ToString()),
                Theme.SubtitleFont,
                Theme.TextDarkBrush,
                new Point(activeArea.Left, top),
                StringFormat.GenericTypographic);

            //debug notice
#if DEBUG
            top += (int)sz.Height;

            e.Graphics.DrawString(
                "[Debug Compilation]",
                Theme.SubtitleFont,
                Theme.TextDarkBrush,
                new Point(activeArea.Left, top),
                StringFormat.GenericTypographic);
#endif
     
            //status string
            if (Status.Length > 0)
            {
                sz = e.Graphics.MeasureString(
                    Status,
                    Font,
                    activeArea.Width,
                    StringFormat.GenericTypographic);
                e.Graphics.DrawString(
                    Status,
                    Font,
                    Theme.TextDarkBrush,
                    new PointF(activeArea.Left, progressBar.Top-sz.Height*1.5f),
                    StringFormat.GenericTypographic);
            }
        }

        protected override void OnFirstShown(EventArgs e)
        {
            base.OnFirstShown(e);
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

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            RecalculateActiveArea();
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