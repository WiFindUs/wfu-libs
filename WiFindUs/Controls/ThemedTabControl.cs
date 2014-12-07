using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using WiFindUs.Extensions;

namespace WiFindUs.Controls
{
    public class ThemedTabControl : TabControl, IThemeable
    {
        private Theme theme;
        private int hoverIndex = -1;
        
        /////////////////////////////////////////////////////////////////////
        // PROPERTIES
        /////////////////////////////////////////////////////////////////////

        public int HoverIndex
        {
            get { return hoverIndex; }
            protected set
            {
                if (value == hoverIndex)
                    return;
                hoverIndex = value;
                Refresh();
            }
        }

        public bool IsDesignMode
        {
            get
            {
                return DesignMode || this.IsDesignMode();
            }
        }

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
                foreach (TabPage tab in TabPages)
                {
                    tab.BackColor = theme.ControlMidColour;
                    tab.Font = theme.WindowFont;
                }
            }
        }
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
            ResizeRedraw = true;

            if (IsDesignMode)
                theme = WFUApplication.Theme;

            DoubleBuffered = true;
            SetStyle(
                System.Windows.Forms.ControlStyles.UserPaint |
                System.Windows.Forms.ControlStyles.AllPaintingInWmPaint |
                System.Windows.Forms.ControlStyles.OptimizedDoubleBuffer,
                true);
        }

        /////////////////////////////////////////////////////////////////////
        // PROTECTED METHODS
        /////////////////////////////////////////////////////////////////////

        protected override void OnPaint(PaintEventArgs e)
        {
            //background
            base.OnPaint(e);
            PaintTransparentBackground(e.Graphics, ClientRectangle);
            
            //highlight line
            Rectangle tabBounds = TabBounds;
            Rectangle highlightRect = new Rectangle(
                //left
                Alignment == TabAlignment.Top || Alignment == TabAlignment.Bottom ? ClientRectangle.Left
                    : (Alignment == TabAlignment.Left ? tabBounds.Right : tabBounds.Left),
                //top
                Alignment == TabAlignment.Left || Alignment == TabAlignment.Right ? ClientRectangle.Top
                    : (Alignment == TabAlignment.Top ? tabBounds.Bottom : tabBounds.Top),
                //width
                Alignment == TabAlignment.Top || Alignment == TabAlignment.Bottom ? ClientRectangle.Width : 2,
                //height
                Alignment == TabAlignment.Left || Alignment == TabAlignment.Right ? ClientRectangle.Height : 2);
            e.Graphics.FillRectangle(Theme.HighlightMidBrush, highlightRect);

            //tabs
            if (!IsDesignMode)
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
                if ( GetTabRect(i).Contains(e.Location))
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

        private void PaintTab(Graphics g, int tabIndex)
        {
            Rectangle tabTextArea = GetTabRect(tabIndex);

            //draw background (if selected or hovering
            if (hoverIndex == tabIndex || SelectedIndex == tabIndex)
                g.FillRectangle(hoverIndex == tabIndex ? Theme.HighlightLightBrush : Theme.HighlightMidBrush, tabTextArea);

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
                Theme.TextLightBrush,
                new PointF(tabTextArea.Left + 4.0f, tabTextArea.Top + tabTextArea.Height / 2.0f - sz.Height/2.0f),
                StringFormat.GenericTypographic);
        }
    }
}
