using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace WiFindUs.Forms
{
    public class MainForm : BaseForm
	{
        /////////////////////////////////////////////////////////////////////
        // PROPERTIES
        /////////////////////////////////////////////////////////////////////

        protected virtual List<Func<bool>> LoadingTasks
        {
            get { return new List<Func<bool>>(); }
        }

        protected override bool ShowWithoutActivation
        {
            get { return false; }
        }

        /////////////////////////////////////////////////////////////////////
        // CONSTRUCTORS
        /////////////////////////////////////////////////////////////////////
		
		public MainForm()
        {
            if (IsDesignMode)
                return;

            ShowInTaskbar = false;
            WindowState = FormWindowState.Minimized;
        }

        /////////////////////////////////////////////////////////////////////
        // PROTECTED METHODS
        /////////////////////////////////////////////////////////////////////

        protected override void SetVisibleCore(bool value)
        {
            base.SetVisibleCore(IsDesignMode || WFUApplication.SplashLoadingFinished ? value : false);
        }

        protected override void OnFirstShown(EventArgs e)
        {
            base.OnFirstShown(e);
            Debugger.FlushToConsoles();
        }

        /////////////////////////////////////////////////////////////////////
        // PRIVATE METHODS
        /////////////////////////////////////////////////////////////////////
	}
}
