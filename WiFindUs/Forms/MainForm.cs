using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace WiFindUs
{
	public class MainForm : Form
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

        /////////////////////////////////////////////////////////////////////
        // CONSTRUCTORS
        /////////////////////////////////////////////////////////////////////
		
		public MainForm()
        {
            if (DesignMode)
                return;

			if (WFUApplication.UsesConsoleForm)
			{
				Debugger.V("Creating console form...");
				console = new ConsoleForm();
				Debugger.V("Created OK.");
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

        /////////////////////////////////////////////////////////////////////
        // PUBLIC METHODS
        /////////////////////////////////////////////////////////////////////

		public virtual void ShowForm(bool forcerefresh = true)
		{
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
			console.Focus();
		}

        /////////////////////////////////////////////////////////////////////
        // PROTECTED METHODS
        /////////////////////////////////////////////////////////////////////

        protected virtual void OnFirstShown(EventArgs e)
        {
            if (WFUApplication.UsesConsoleForm)
                ShowConsoleForm();
        }

		protected override void OnShown(EventArgs e)
		{
			base.OnShown(e);
			if (DesignMode)
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
			if (DesignMode)
				return;
			Debugger.V("Assigning icon from WFUApplication.Icon...");
			Icon = WFUApplication.Icon;
			Text = WFUApplication.Name + " v" + WFUApplication.AssemblyVersion;
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
	}
}
