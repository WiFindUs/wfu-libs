using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using WiFindUs.Extensions;

namespace WiFindUs.Themes
{
    public class ThemedTabControl : TabControl, IThemeable
	{
		private int hoverIndex = -1;

		/////////////////////////////////////////////////////////////////////
		// PROPERTIES
		/////////////////////////////////////////////////////////////////////

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public int HoverIndex
		{
			get { return hoverIndex; }
			protected set
			{
				if (value == hoverIndex)
					return;
				hoverIndex = value;
				InvalidateNonPanelArea();
			}
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool IsDesignMode
		{
			get { return DesignMode || this.IsDesignMode(); }
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		protected Rectangle TabBounds
		{
			get
			{
				if (TabPages.Count == 0)
					return Rectangle.Empty;
				Rectangle first = GetTabRect(0);
				Rectangle last = GetTabRect(TabPages.Count - 1);
				return new Rectangle(first.Left, first.Top, last.Right - first.Left, last.Bottom - first.Top);
			}
		}

		/////////////////////////////////////////////////////////////////////
		// CONSTRUCTORS
		/////////////////////////////////////////////////////////////////////

		public ThemedTabControl()
		{
			DrawMode = TabDrawMode.OwnerDrawFixed;
			SizeMode = TabSizeMode.Fixed;
			ItemSize = new Size(80, 30);

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
			Font = theme.Controls.Normal.Regular;
			foreach (TabPage tab in TabPages)
			{
				tab.BackColor = theme.Background.Mid.Colour;
				tab.Font = Font;
			}
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

		protected override void OnPaint(PaintEventArgs e)
		{
			//background
			base.OnPaint(e);
			PaintTransparentBackground(e.Graphics, ClientRectangle);

			bool design = IsDesignMode;

			//highlight line
			Rectangle tabBounds = TabBounds;
			int offset = Alignment == TabAlignment.Top || Alignment == TabAlignment.Bottom
				? tabBounds.Left - ClientRectangle.Left : 0;
			Rectangle highlightRect = new Rectangle(
				//left
				Alignment == TabAlignment.Top || Alignment == TabAlignment.Bottom
					? ClientRectangle.Left + offset: (Alignment == TabAlignment.Left ? tabBounds.Right : tabBounds.Left),
				//top
				Alignment == TabAlignment.Left || Alignment == TabAlignment.Right
					? ClientRectangle.Top : (Alignment == TabAlignment.Top ? tabBounds.Bottom : tabBounds.Top),
				//width
				Alignment == TabAlignment.Top || Alignment == TabAlignment.Bottom
					? ClientRectangle.Width - offset * 2 : 2,
				//height
				Alignment == TabAlignment.Left || Alignment == TabAlignment.Right
					? ClientRectangle.Height : 2
				);
			e.Graphics.FillRectangle(design
				? SystemBrushes.ControlDark
				: (ContainsFocus ? Theme.Current.Highlight.Mid.Brush : Theme.Current.Background.Light.Brush), highlightRect);

			//tabs
			if (!design)
				e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
			for (int i = 0; i < TabPages.Count; i++)
				PaintTab(e.Graphics, i);
		}

		protected void PaintTransparentBackground(Graphics graphics, Rectangle clipRect)
		{
			graphics.Clear(Color.Transparent);
			if ((this.Parent != null))
			{
				clipRect.Offset(this.Location);
				PaintEventArgs e = new PaintEventArgs(graphics, clipRect);
				GraphicsState state = graphics.Save();
				graphics.SmoothingMode = SmoothingMode.HighSpeed;
				try
				{
					graphics.TranslateTransform((float)-this.Location.X, (float)-this.Location.Y);
					this.InvokePaintBackground(this.Parent, e);
					this.InvokePaint(this.Parent, e);
				}
				finally
				{
					graphics.Restore(state);
					clipRect.Offset(-this.Location.X, -this.Location.Y);
				}
			}
		}

		protected override void OnMouseMove(MouseEventArgs e)
		{
			base.OnMouseMove(e);
			int idx = -1;
			for (int i = 0; i < TabCount; i++)
			{
				if (GetTabRect(i).Contains(e.Location))
				{
					idx = i;
					break;
				}
			}
			HoverIndex = idx;
		}

		protected override void OnMouseLeave(EventArgs e)
		{
			base.OnMouseLeave(e);
			HoverIndex = -1;
		}

		protected override void OnEnter(EventArgs e)
		{
			base.OnEnter(e);
			InvalidateNonPanelArea();
		}

		protected override void OnLeave(EventArgs e)
		{
			base.OnLeave(e);
			InvalidateNonPanelArea();
		}

		/////////////////////////////////////////////////////////////////////
		// PRIVATE METHODS
		/////////////////////////////////////////////////////////////////////

		private void PaintTab(Graphics g, int tabIndex)
		{
			Rectangle tabTextArea = GetTabRect(tabIndex);
			bool design = IsDesignMode;

			//draw background (if selected or hovering)
			if (design)
				g.FillRectangle(SystemBrushes.Control, tabTextArea);
			else if (hoverIndex == tabIndex || SelectedIndex == tabIndex)
				g.FillRectangle(hoverIndex == tabIndex
					? (ContainsFocus ? Theme.Current.Highlight.Light.Brush : Theme.Current.Background.Lighter.Brush)
					: (ContainsFocus ? Theme.Current.Highlight.Mid.Brush : Theme.Current.Background.Light.Brush), tabTextArea);

			//draw text
			string text = TabPages[tabIndex].Text;
			SizeF sz = g.MeasureString(
				text,
				Font,
				ItemSize.Width,
				StringFormat.GenericTypographic);
			g.DrawString(
				text,
				Font,
				design ? SystemBrushes.ControlText : Theme.Current.Foreground.Lighter.Brush,
				new PointF(tabTextArea.Left + 4.0f, tabTextArea.Top + tabTextArea.Height / 2.0f - sz.Height / 2.0f),
				StringFormat.GenericTypographic);
		}

		private void InvalidateNonPanelArea()
		{
			System.Drawing.Region region = new System.Drawing.Region(Bounds);
			if (TabPages.Count > 0)
				region.Xor(SelectedTab.Bounds);
			Invalidate(region);
			Update();
		}
	}
}
