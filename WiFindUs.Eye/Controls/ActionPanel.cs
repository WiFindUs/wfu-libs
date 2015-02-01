using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WiFindUs.Controls;

namespace WiFindUs.Eye.Controls
{
    public class ActionPanel : ThemedPanel
    {
        private IActionSubscriber actionSubscriber;
        private uint rows, cols;
        private Button[,] buttons;
        private int activeButtons = 0;
        private Rectangle descriptionRectangle;
        private const int BUTTON_SPACING = 2;

        /////////////////////////////////////////////////////////////////////
        // PROPERTIES
        /////////////////////////////////////////////////////////////////////

        public event Action<ActionPanel, IActionSubscriber> ActionSubscriberChanged;

        public IActionSubscriber ActionSubscriber
        {
            get { return actionSubscriber; }
            set
            {
                if (value == actionSubscriber)
                    return;
                actionSubscriber = value;

                activeButtons = 0;
                for (uint r = 0; r < rows; r++)
                    for (uint c = 0; c < cols; c++)
                    {
                        Button b = buttons[r, c];
                        uint index = (uint)b.Tag;
                        if (actionSubscriber == null)
                        {
                            b.Visible = false;
                            b.Image = null;
                        }
                        else
                        {
                            activeButtons++;
                            b.Visible = true;
                            b.Image = actionSubscriber.ActionImage(index);
                            b.Text = actionSubscriber.ActionText(index);
                        }
                    }

                Refresh();
                if (ActionSubscriberChanged != null)
                    ActionSubscriberChanged(this, actionSubscriber);
            }
        }

        public uint Rows
        {
            get { return rows; }
        }

        public uint Columns
        {
            get { return cols; }
        }

        public uint Capacity
        {
            get { return rows * cols; }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override Theme Theme
        {
            get
            {
                return base.Theme;
            }
            set
            {
                if (value == null || value == base.Theme)
                    return;

                base.Theme = value;

                if (buttons == null)
                    return;

                for (int r = 0; r < rows; r++)
                    for (int c = 0; c < cols; c++)
                    {
                        Button b = buttons[r, c];
                        b.BackColor = base.Theme.ControlMidColour;
                    }

            }
        }

        /////////////////////////////////////////////////////////////////////
        // CONSTRUCTORS
        /////////////////////////////////////////////////////////////////////

        public ActionPanel(uint rows, uint cols)
            : base()
        {
            if (IsDesignMode)
                return;
            if (rows == 0)
                throw new ArgumentOutOfRangeException("rows", "Rows must be greater than zero.");
            if (cols == 0)
                throw new ArgumentOutOfRangeException("cols", "Columns must be greater than zero.");
            this.cols = cols;
            this.rows = rows;
            buttons = new Button[rows, cols];
            
            SuspendLayout();
            uint index = 0;
            for (int r = 0; r < rows; r++)
                for (int c = 0; c < cols; c++)
                {
                    Button b;
                    Controls.Add(b = buttons[r, c] = new Button()
                    {
                        FlatStyle = FlatStyle.Flat,
                        Anchor = AnchorStyles.None,
                        Visible = false,
                        Tag = index
                    });
                    b.FlatAppearance.BorderSize = 0;
                    b.Click += ActionButtonClicked;
                    index++;
                }
            PositionButtons();
            ResumeLayout(false);
        }

        /////////////////////////////////////////////////////////////////////
        // PROTECTED METHODS
        /////////////////////////////////////////////////////////////////////

        protected override void OnResize(EventArgs eventargs)
        {
            base.OnResize(eventargs);
            SuspendLayout();
            PositionButtons();
            ResumeLayout(false);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            if (IsDesignMode)
                return;

            if (actionSubscriber != null && activeButtons > 0)
            {
                string text = actionSubscriber.ActionDescription;
                var sizeText = e.Graphics.MeasureString(text, Font, descriptionRectangle.Width, StringFormat.GenericTypographic);
                e.Graphics.DrawString(text, Font, Theme.TextMidBrush,
                    (descriptionRectangle.Width - sizeText.Width) / 2,
                    (descriptionRectangle.Height - sizeText.Height) / 2,
                    StringFormat.GenericTypographic);
            }
            else
            {
                string text = "No actions available. Try selecting something.";
                var sizeText = e.Graphics.MeasureString(text, Font, ClientRectangle.Width, StringFormat.GenericTypographic);
                e.Graphics.DrawString(text, Font, Theme.TextMidBrush,
                    (ClientRectangle.Width - sizeText.Width) / 2,
                    (ClientRectangle.Height - sizeText.Height) / 2,
                    StringFormat.GenericTypographic);
            }

        }

        protected override void OnFontChanged(EventArgs e)
        {
            base.OnFontChanged(e);
            SuspendLayout();
            PositionButtons();
            ResumeLayout(false);
        }

        /////////////////////////////////////////////////////////////////////
        // PRIVATE METHODS
        /////////////////////////////////////////////////////////////////////

        private void PositionButtons()
        {
            int descHeight = TextRenderer.MeasureText("abcABCqFz", Font).Height;
            descriptionRectangle = new Rectangle(0, 0, ClientRectangle.Width, 5 * descHeight / 4);

            int width = (int)((ClientRectangle.Width - (cols + 1) * BUTTON_SPACING) / (float)cols);
            int height = (int)(((ClientRectangle.Height - (rows + 1) * BUTTON_SPACING) - descriptionRectangle.Height) / (float)rows);
            for (int r = 0; r < rows; r++)
                for (int c = 0; c < cols; c++)
                    buttons[r, c].Bounds = new System.Drawing.Rectangle(
                        (BUTTON_SPACING * (c + 1) + width * c),
                        (BUTTON_SPACING * (r + 1) + height * r) + descriptionRectangle.Height,
                        width,
                        height);
        }

        private void ActionButtonClicked(object sender, EventArgs e)
        {
            if (actionSubscriber == null)
                return;

            Button b = sender as Button;
            if (b == null || !b.Enabled || !b.Visible || b.Tag == null)
                return;
            actionSubscriber.ActionTriggered((uint)b.Tag);
        }
    }
}
