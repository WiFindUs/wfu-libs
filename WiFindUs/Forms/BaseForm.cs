using System;
using System.ComponentModel;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using WiFindUs.Controls;
using WiFindUs.Extensions;

namespace WiFindUs.Forms
{
	public class BaseForm : Form, IThemeable
	{
		private Theme theme;
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

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool IsDesignMode
		{
			get
			{
				return DesignMode || this.IsDesignMode();
			}
		}

		protected override bool ShowWithoutActivation
		{
			get { return true; }
		}

		protected override CreateParams CreateParams
		{
			get
			{
				CreateParams cp = base.CreateParams;
				if (IsDesignMode)
					return cp;
				cp.ExStyle |= 0x02000000;  // Turn on WS_EX_COMPOSITED
				return cp;
			}
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public Theme Theme
		{
			get
			{
				return theme;
			}
			set
			{
				if (value == null || value == theme)
					return;

				theme = value;
				BackColor = theme.ControlLightColour;
				Font = theme.WindowFont;
				OnThemeChanged();
				this.RecurseControls(control =>
				{
					IThemeable themable = control as IThemeable;
					if (themable != null)
						themable.Theme = value;
				});
				Refresh();
			}
		}

		/////////////////////////////////////////////////////////////////////
		// CONSTRUCTORS
		/////////////////////////////////////////////////////////////////////

		public BaseForm()
		{
			AutoScaleMode = AutoScaleMode.None;
			ShowIcon = true;
			ResizeRedraw = true;

			if (IsDesignMode)
			{
				theme = WFUApplication.Theme;
				return;
			}

			Icon = WFUApplication.Icon;
			if (WFUApplication.UIThreadID < 0)
				WFUApplication.UIThreadID = Thread.CurrentThread.ManagedThreadId;

			DoubleBuffered = true;
			SetStyle(
				System.Windows.Forms.ControlStyles.UserPaint |
				System.Windows.Forms.ControlStyles.AllPaintingInWmPaint |
				System.Windows.Forms.ControlStyles.OptimizedDoubleBuffer,
				true);
			UpdateStyles();
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

		public virtual void OnThemeChanged()
		{

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

		protected override void OnLoad(EventArgs e)
		{
			if (IsDesignMode)
				return;
			base.OnLoad(e);
			Theme = WFUApplication.Theme;
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
