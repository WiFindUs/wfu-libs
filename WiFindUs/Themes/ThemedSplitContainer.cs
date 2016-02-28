using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using WiFindUs.Extensions;

namespace WiFindUs.Themes
{
    public class ThemedSplitContainer : SplitContainer, IThemeable
	{
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
			get { return DesignMode || this.IsDesignMode(); }
		}

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public Rectangle HandleBounds
		{
			get
			{
				Rectangle sb = SplitterRectangle;
				int length = Math.Max(Math.Max(sb.Width, sb.Height) / 8, 40);
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
			if (theme == null)
				return;
			//Panel1.BackColor = theme.Background.Light.Colour;
			//Panel1.ForeColor = theme.Foreground.Light.Colour;
			//Panel1.Font = theme.Controls.Normal.Regular;
			
			//Panel2.BackColor = theme.Background.Light.Colour;
			//Panel2.ForeColor = theme.Foreground.Light.Colour;
			//Panel2.Font = theme.Controls.Normal.Regular;
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

		protected override void OnMouseMove(MouseEventArgs e)
		{
			base.OnMouseMove(e);
			MouseOverSplitter = SplitterRectangle.Contains(e.Location);
		}

		/////////////////////////////////////////////////////////////////////
		// PRIVATE METHODS
		/////////////////////////////////////////////////////////////////////

		private void InvalidateNonPanelArea()
		{
			System.Drawing.Region region = new System.Drawing.Region(Bounds);
			region.Xor(Panel1.Bounds);
			region.Xor(Panel2.Bounds);
			Invalidate(region);
			Update();
		}
	}
}
