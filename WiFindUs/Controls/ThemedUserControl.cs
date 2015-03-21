using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WiFindUs.Extensions;

namespace WiFindUs.Controls
{
    public class ThemedUserControl : UserControl, IThemeable
    {
        public event Action<ThemedUserControl> MouseHoveringChanged;
        private Theme theme;
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

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual Theme Theme
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
                OnThemeChanged();
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
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

        public ThemedUserControl()
        {
            Margin = new Padding(0);
            Padding = new Padding(0);

            if (IsDesignMode)
                return;

            ResizeRedraw = false;
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

        public virtual void OnThemeChanged()
        {

        }

        /////////////////////////////////////////////////////////////////////
        // PROTECTED METHODS
        /////////////////////////////////////////////////////////////////////

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
