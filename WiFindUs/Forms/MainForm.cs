using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Drawing;

namespace WiFindUs.Forms
{
    public class MainForm : BaseForm, ISplashLoader
	{
        /////////////////////////////////////////////////////////////////////
        // PROPERTIES
        /////////////////////////////////////////////////////////////////////

        public virtual List<Func<bool>> LoadingTasks
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
            Screen startScreen = null;
            int configMonitor = WFUApplication.Config.Get("display.start_screen", -1);
            Debugger.V("Start screen: " + configMonitor);
            if (configMonitor >= 0)
            {
                foreach (Screen screen in Screen.AllScreens)
                {
                    if (screen.DeviceName.CompareTo("\\\\.\\DISPLAY" + configMonitor) == 0)
                    {
                        startScreen = screen;
                        break;
                    }
                }
                if (startScreen == null)
                    Debugger.W("Could not find screen matching given number; will use primary screen.");
            }
            if (startScreen == null)
                startScreen = Screen.PrimaryScreen;
            Location = new Point(startScreen.Bounds.Left + 10, startScreen.Bounds.Top + 10);
            bool startMaximized = WFUApplication.Config.Get("display.start_maximized", false);
            Debugger.V("Start maximized: " + startMaximized);
            if (startMaximized)
                WindowState = FormWindowState.Maximized;
        }

        /////////////////////////////////////////////////////////////////////
        // PRIVATE METHODS
        /////////////////////////////////////////////////////////////////////
	}
}
