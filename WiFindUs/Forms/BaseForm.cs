using System;
using System.ComponentModel;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using WiFindUs.Controls;
using WiFindUs.Extensions;
using WiFindUs.Themes;

namespace WiFindUs.Forms
{
	public class BaseForm : ThemedForm
	{
		private bool firstShown = false;
		private bool hideOnClose = false;
		private bool preventUserClose = false;
		private static readonly Regex TITLE_ADMIN
			= new Regex(@"\(Administrator\)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
#if DEBUG
		private static readonly Regex TITLE_DEBUG
			= new Regex(@"\(Debug build\)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
#endif

		/////////////////////////////////////////////////////////////////////
		// PROPERTIES
		/////////////////////////////////////////////////////////////////////

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool FirstShown
		{
			get { return firstShown; }
		}

		public bool HideOnClose
		{
			get { return hideOnClose; }
			set { hideOnClose = value; }
		}

		public bool PreventUserClose
		{
			get { return preventUserClose; }
			set { preventUserClose = value; }
		}

		protected override bool ShowWithoutActivation
		{
			get { return true; }
		}

		/////////////////////////////////////////////////////////////////////
		// CONSTRUCTORS
		/////////////////////////////////////////////////////////////////////

		public BaseForm()
		{
			AutoScaleMode = AutoScaleMode.None;
			ShowIcon = true;

			if (IsDesignMode)
				return;

			Icon = WFUApplication.Icon;
			WFUApplication.SetThreadAlias("UI");
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

		public void ApplyWindowStateFromConfig(String prefix)
		{
			if (prefix == null || prefix.Length == 0 || WFUApplication.Config == null)
				return;
			Screen startScreen = null;

			//check for keywords
			string screenKeyword = WFUApplication.Config.Get(prefix + ".start_screen", "");
			if ((screenKeyword = screenKeyword.Trim().ToLower()).Length > 0)
			{
				switch (screenKeyword)
				{
					case "one":
					case "first":
						startScreen = Screen.AllScreens[0];
						break;

					case "two":
					case "second":
						startScreen = Screen.AllScreens.Length > 1 ? Screen.AllScreens[1] : null;
						break;

					case "three":
					case "third":
						startScreen = Screen.AllScreens.Length > 2 ? Screen.AllScreens[2] : null;
						break;

					case "fourth":
					case "four":
						startScreen = Screen.AllScreens.Length > 3 ? Screen.AllScreens[3] : null;
						break;

					case "main":
					case "primary":
						startScreen = Screen.PrimaryScreen;
						break;
				}
			}

			//check screen number
			if (startScreen == null)
			{
				int configMonitor = WFUApplication.Config.Get(prefix + ".start_screen", -1);
				Debugger.V("Start screen (" + prefix + "): " + configMonitor);
				if (configMonitor >= 0)
					startScreen = Screen.AllScreens.Length > configMonitor ? Screen.AllScreens[configMonitor] : null;
			}

			//use primary by default
			if (startScreen == null)
			{
				Debugger.W("Could not find screen matching given criteria; will use primary screen.");
				startScreen = Screen.PrimaryScreen;
			}
			Location = new Point(startScreen.Bounds.Left + 10, startScreen.Bounds.Top + 10);
			bool startMaximized = WFUApplication.Config.Get(prefix + ".start_maximized", false);
			Debugger.V("Start maximized (" + prefix + "): " + startMaximized);
			if (startMaximized)
				WindowState = FormWindowState.Maximized;
		}

		/////////////////////////////////////////////////////////////////////
		// PROTECTED METHODS
		/////////////////////////////////////////////////////////////////////

		protected override void OnFormClosing(FormClosingEventArgs e)
		{
			if (e.CloseReason == CloseReason.UserClosing)
			{
				if (preventUserClose || hideOnClose)
					e.Cancel = true;
				if (hideOnClose && !preventUserClose)
					Hide();
			}
			if (!e.Cancel)
				base.OnFormClosing(e);
		}

		protected override void OnShown(EventArgs e)
		{
			if (IsDesignMode)
				return;
			if (!firstShown)
			{
				firstShown = true;
				OnFirstShown(e);
			}
			base.OnShown(e);
		}

		protected virtual void OnFirstShown(EventArgs e)
		{

		}

		protected virtual void OnDisposing()
		{

		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
				OnDisposing();
			base.Dispose(disposing);
		}

#if DEBUG
		protected override void OnTextChanged(EventArgs e)
		{
			string text = Text;

			//check for app name
			if (!text.Contains(WFUApplication.Name))
				text += " - " + WFUApplication.Name;

			//check for admin flag
			if (!TITLE_ADMIN.IsMatch(text))
				text += " (Administrator)";

#if DEBUG
			//check for debug flag
			if (!TITLE_DEBUG.IsMatch(text))
				text += " (Debug build)";
#endif
			
			//push change
			if (text.CompareTo(Text) != 0)
				Text = text;
			else
				base.OnTextChanged(e);
		}
#endif
	}
}
