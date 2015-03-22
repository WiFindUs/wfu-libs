using System.ComponentModel;
using System.Windows.Forms;
using WiFindUs.Extensions;

namespace WiFindUs.Controls
{
    public class ThemedFlowLayoutPanel : FlowLayoutPanel, IThemeable
    {
        private Theme theme;

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

                OnThemeChanged();
            }
        }

        /////////////////////////////////////////////////////////////////////
        // CONSTRUCTORS
        /////////////////////////////////////////////////////////////////////

        public ThemedFlowLayoutPanel()
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
    }
}
