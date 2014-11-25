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
		private ConsoleForm console = null;
		private NotifyIcon trayIcon = null;
		private ToolStripMenuItem exitItem = null, consoleItem = null;
		private bool firstShown = false;

        /////////////////////////////////////////////////////////////////////
        // PROPERTIES
        /////////////////////////////////////////////////////////////////////

        public ConsoleForm Console
        {
            get { return console; }
        }
        protected ToolStripMenuItem ExitItem
        {
            get { return exitItem; }
        }
        protected ToolStripMenuItem ConsoleItem
        {
            get { return consoleItem; }
        }
        protected NotifyIcon TrayIcon
        {
            get { return trayIcon; }
        }
        protected ContextMenuStrip TrayIconMenu
        {
            get { return trayIcon == null ? null : trayIcon.ContextMenuStrip; }
        }

        protected virtual List<Func<bool>> LoadingTasks
        {
            get
            {
                return new List<Func<bool>>();
            }
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

            Icon = WFUApplication.Icon;
            Text = WFUApplication.Name + " v" + WFUApplication.AssemblyVersion;
        }

        /////////////////////////////////////////////////////////////////////
        // PUBLIC METHODS
        /////////////////////////////////////////////////////////////////////

		public virtual void ShowForm(bool forcerefresh = true)
		{
            Visible = true;
            Show();
			BringToFront();
			Focus();
			if (forcerefresh)
				Refresh();
		}

		public void ShowConsoleForm()
		{
			if (console == null)
				return;
			Debugger.V("Showing console form...");
			console.Show();
			console.BringToFront();
		}

        /////////////////////////////////////////////////////////////////////
        // PROTECTED METHODS
        /////////////////////////////////////////////////////////////////////

        protected override void SetVisibleCore(bool value)
        {
            base.SetVisibleCore(WFUApplication.SplashLoadingFinished ? value : false);
        }

        protected virtual void OnFirstShown(EventArgs e)
        {
            if (WFUApplication.UsesConsoleForm)
            {
                Debugger.V("Creating console form...");
                console = new ConsoleForm();
                console.Owner = this;
                Debugger.V("Created OK.");
                ShowConsoleForm();
            }

            if (WFUApplication.UsesTrayIcon)
            {
                Debugger.I("Creating tray icon...");
                trayIcon = new NotifyIcon();
                trayIcon.Text = WFUApplication.Name + " v" + WFUApplication.AssemblyVersion;
                trayIcon.Icon = WFUApplication.Icon;

                exitItem = new ToolStripMenuItem("Exit " + WFUApplication.Name);
                exitItem.Click += TrayIconCloseClick;

                ContextMenuStrip trayIconMenu = new ContextMenuStrip();
                trayIconMenu.Items.Add(new ToolStripSeparator());
                if (WFUApplication.UsesConsoleForm)
                {
                    consoleItem = new ToolStripMenuItem("Console");
                    consoleItem.Click += TrayIconConsoleClick;
                    trayIconMenu.Items.Add(consoleItem);
                }
                trayIconMenu.Items.Add(exitItem);
                trayIcon.ContextMenuStrip = trayIconMenu;

                trayIcon.Visible = true;
                trayIcon.DoubleClick += TrayIconDoubleClick;
                Debugger.V("Tray icon created OK.");
            }
        }

		protected override void OnShown(EventArgs e)
		{
			base.OnShown(e);
            if (IsDesignMode)
				return;
			if (!firstShown)
			{
				firstShown = true;
                OnFirstShown(e);
			}
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
            if (IsDesignMode)
				return;
		}

        protected virtual void TrayIconDoubleClick(object sender, EventArgs e)
        {
            ShowForm();
        }

		protected virtual void TrayIconCloseClick(object sender, EventArgs e)
		{
			Close();
		}

		protected virtual void TrayIconConsoleClick(object sender, EventArgs e)
		{
			if (console != null)
				ShowConsoleForm();
		}

		protected override void OnFormClosing(FormClosingEventArgs e)
		{
			if (trayIcon != null && (e.Cancel = (e.CloseReason == CloseReason.UserClosing)))
				Hide();
			else
				base.OnFormClosing(e);
		}

		protected override void OnFormClosed(FormClosedEventArgs e)
		{
			if (console != null)
			{
				console.Dispose();
				console = null;
			}

			if (trayIcon != null)
			{
				trayIcon.Visible = false;
				trayIcon.Dispose();
				trayIcon = null;
			}

			base.OnFormClosed(e);
		}

        /////////////////////////////////////////////////////////////////////
        // PRIVATE METHODS
        /////////////////////////////////////////////////////////////////////
	}
}
