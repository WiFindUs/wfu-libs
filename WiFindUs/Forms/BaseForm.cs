using System;
using System.ComponentModel;
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

		/////////////////////////////////////////////////////////////////////
		// PROPERTIES
		/////////////////////////////////////////////////////////////////////

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
				if (!IsDesignMode)
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
			WFUApplication.UIThreadID = Thread.CurrentThread.ManagedThreadId;
			AutoScaleMode = AutoScaleMode.None;
			ShowIcon = true;
			Icon = WFUApplication.Icon;
			ResizeRedraw = true;
			Text = WFUApplication.Name;

			if (IsDesignMode)
			{
				theme = WFUApplication.Theme;
				return;
			}

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

		/////////////////////////////////////////////////////////////////////
		// PUBLIC METHODS
		/////////////////////////////////////////////////////////////////////

		public virtual void OnThemeChanged()
		{

		}

		/////////////////////////////////////////////////////////////////////
		// PROTECTED METHODS
		/////////////////////////////////////////////////////////////////////

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			if (IsDesignMode)
				return;

			Theme = WFUApplication.Theme;
			Refresh();
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
	}
}
