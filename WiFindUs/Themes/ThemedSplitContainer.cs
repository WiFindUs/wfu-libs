using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WiFindUs.Extensions;

namespace WiFindUs.Themes
{
	public class ThemedSplitContainer : SplitContainer, IThemeable
	{
		public event Action<ThemedSplitContainer> MouseHoveringChanged;
		private bool mouseHovering = false;
		private bool mouseOverSplitter = false;

		/////////////////////////////////////////////////////////////////////
		// PROPERTIES
		/////////////////////////////////////////////////////////////////////

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool MouseOverSplitter
		{
			get { return mouseOverSplitter; }
			protected set
			{
				if (mouseOverSplitter == value)
					return;
				mouseOverSplitter = value;
				InvalidateNonPanelArea();
			}
		}

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool IsDesignMode
		{
			get
			{
				return DesignMode || this.IsDesignMode();
			}
		}

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
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

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public Rectangle SplitterBounds
		{
			get
			{
				if (Orientation == Orientation.Vertical)
					return new Rectangle(Panel1.Right, ClientRectangle.Top, Panel2.Left - Panel1.Right, ClientRectangle.Height);
				return new Rectangle(ClientRectangle.Left, Panel1.Bottom, ClientRectangle.Width, Panel2.Top - Panel1.Bottom);
			}
		}

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public Rectangle HandleBounds
		{
			get
			{
				Rectangle sb = SplitterBounds;
				int length = Math.Max(Math.Max(sb.Width, sb.Height) / 10, 40);
				if (Orientation == Orientation.Vertical)
					return new Rectangle(sb.Left, sb.Top + sb.Height / 2 - length / 2, sb.Width, length);
				return new Rectangle(sb.Left + sb.Width / 2 - length / 2, sb.Top, length, sb.Height);
			}
		}

		/////////////////////////////////////////////////////////////////////
		// CONSTRUCTORS
		/////////////////////////////////////////////////////////////////////

		public ThemedSplitContainer()
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
			//if (theme == null)
			//	return;
			//BackColor = theme.Background.Light.Colour;
			//ForeColor = theme.Foreground.Light.Colour;
			//Font = theme.Controls.Normal.Regular;
		}

		/////////////////////////////////////////////////////////////////////
		// PROTECTED METHODS
		/////////////////////////////////////////////////////////////////////

		protected override void OnPaint(PaintEventArgs e)
		{
			base.OnPaint(e);
			if (IsDesignMode)
				return;

			//draw handle
			if (!IsSplitterFixed)
			{
				Rectangle handle = HandleBounds;

				Pen pen = mouseOverSplitter ? Theme.Current.Highlight.Mid.Pen
					: Theme.Current.Background.Light.Pen;

				if (Orientation == Orientation.Vertical)
				{
					e.Graphics.DrawLine(pen, handle.Left, handle.Top, handle.Left, handle.Bottom);
					e.Graphics.DrawLine(pen, handle.Right-1, handle.Top, handle.Right-1, handle.Bottom);
				}
				else
				{
					e.Graphics.DrawLine(pen, handle.Left, handle.Top, handle.Right, handle.Top);
					e.Graphics.DrawLine(pen, handle.Left, handle.Bottom-1, handle.Right, handle.Bottom-1);
				}
			}
		}

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
			MouseOverSplitter = false;
		}

		protected override void OnMouseMove(MouseEventArgs e)
		{
			base.OnMouseMove(e);
			MouseOverSplitter = SplitterBounds.Contains(e.Location);
		}

		/////////////////////////////////////////////////////////////////////
		// PRIVATE METHODS
		/////////////////////////////////////////////////////////////////////

		private void InvalidateNonPanelArea()
		{
			Region region = new Region(Bounds);
			region.Xor(Panel1.Bounds);
			region.Xor(Panel2.Bounds);
			Invalidate(region);
			Update();
		}
	}
}
