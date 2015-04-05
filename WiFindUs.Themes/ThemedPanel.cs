using System;
using System.ComponentModel;
using System.Windows.Forms;
using WiFindUs.Extensions;

namespace WiFindUs.Themes
{
	public class ThemedPanel : Panel, IThemeable
	{
		public event Action<ThemedPanel> MouseHoveringChanged;
		private bool mouseHovering = false;

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

		public bool MouseHovering
		{
			get { return mouseHovering; }
			private set
			{
				if (value == mouseHovering)
					return;
				mouseHovering = value;
				OnMouseHoverChanged();
			}
		}

		/////////////////////////////////////////////////////////////////////
		// CONSTRUCTORS
		/////////////////////////////////////////////////////////////////////

		public ThemedPanel()
		{
			Margin = new Padding(0);
			Padding = new Padding(0);

			if (IsDesignMode)
				return;

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

		public virtual void ApplyTheme(ITheme theme)
		{
			if (theme == null)
				return;
			BackColor = theme.Background.Light.Colour;
			ForeColor = theme.Foreground.Light.Colour;
			Font = theme.Controls.Normal.Regular;
		}

		/////////////////////////////////////////////////////////////////////
		// PROTECTED METHODS
		/////////////////////////////////////////////////////////////////////

		protected override void OnHandleCreated(EventArgs e)
		{
			base.OnHandleCreated(e);
			if (IsDesignMode)
				return;
			ApplyTheme(Theme.Current);
			Theme.ThemeChanged += ApplyTheme;
		}

		protected virtual void OnMouseHoverChanged()
		{
			if (MouseHoveringChanged != null)
				MouseHoveringChanged(this);
		}

		protected override void OnMouseEnter(EventArgs e)
		{
			MouseHovering = true;
			base.OnMouseEnter(e);
		}

		protected override void OnMouseLeave(EventArgs e)
		{
			base.OnMouseLeave(e);
			MouseHovering = false;
		}
	}
}
