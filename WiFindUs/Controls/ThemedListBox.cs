using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WiFindUs.Extensions;

namespace WiFindUs.Controls
{
    public class ThemedListBox : ListBox, IThemeable
    {
        private Theme theme;

        /////////////////////////////////////////////////////////////////////
        // PROPERTIES
        /////////////////////////////////////////////////////////////////////

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
                ForeColor = theme.TextLightColour;
                Font = theme.WindowFont;
            }
        }

        /////////////////////////////////////////////////////////////////////
        // CONSTRUCTORS
        /////////////////////////////////////////////////////////////////////
        
        public ThemedListBox()
        {
            Margin = new Padding(0);
            DrawMode = DrawMode.OwnerDrawVariable;
            BorderStyle = BorderStyle.None;
            IntegralHeight = false;

            if (IsDesignMode)
                return;
            
            this.SetStyle(
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw |
                ControlStyles.UserPaint,
                true);
        }

        /////////////////////////////////////////////////////////////////////
        // PROTECTED METHODS
        /////////////////////////////////////////////////////////////////////

        protected override void OnDrawItem(DrawItemEventArgs e)
        {
            if (this.Items.Count == 0)
                return;

            e.DrawBackground();
            ThemedListBoxItem displayItem = Items[e.Index] as ThemedListBoxItem;
            if (displayItem != null)
                displayItem.DrawListboxItem(e);
            else
                e.Graphics.DrawString(this.Items[e.Index].ToString(), e.Font, new SolidBrush(e.ForeColor), new PointF(e.Bounds.X, e.Bounds.Y));
            e.DrawFocusRectangle();
        }

        protected override void OnMeasureItem(MeasureItemEventArgs e)
        {
            base.OnMeasureItem(e);
            if (DesignMode || e.Index < 0)
                return;

            ThemedListBoxItem displayItem = Items[e.Index] as ThemedListBoxItem;
            if (displayItem != null)
            e.ItemHeight = displayItem.MeasureItemHeight(this, e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Region iRegion = new Region(e.ClipRectangle);
            e.Graphics.FillRegion(new SolidBrush(this.BackColor), iRegion);
            if (this.Items.Count > 0)
            {
                for (int i = 0; i < this.Items.Count; ++i)
                {
                    System.Drawing.Rectangle irect = this.GetItemRectangle(i);
                    if (e.ClipRectangle.IntersectsWith(irect))
                    {
                        OnDrawItem(new DrawItemEventArgs(e.Graphics, Font,
                            irect, i,
                            ((SelectionMode == SelectionMode.One && SelectedIndex == i)
                            || (SelectionMode == SelectionMode.MultiSimple && SelectedIndices.Contains(i))
                            || (SelectionMode == SelectionMode.MultiExtended && SelectedIndices.Contains(i)))
                                ? DrawItemState.Selected : DrawItemState.Default,
                            this.ForeColor, this.BackColor));
                        iRegion.Complement(irect);
                    }
                }
            }
        }
    }

    public interface ThemedListBoxItem
    {
        int MeasureItemHeight(ThemedListBox host, MeasureItemEventArgs e);
        void DrawListboxItem(DrawItemEventArgs e);
    }
}
