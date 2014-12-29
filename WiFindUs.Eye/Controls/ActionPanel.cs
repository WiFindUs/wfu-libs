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
            if (IsDesignMode || activeButtons > 0)
                return;

            string text = "No actions available. Try selecting something.";
            var sizeText = e.Graphics.MeasureString(text, Font);
            e.Graphics.DrawString(text, Font, Theme.TextMidBrush,
                (ClientRectangle.Width - sizeText.Width) / 2,
                (ClientRectangle.Height - sizeText.Height) / 2);
        }

        /////////////////////////////////////////////////////////////////////
        // PRIVATE METHODS
        /////////////////////////////////////////////////////////////////////

        private void PositionButtons()
        {
            int space = 2;
            int width = (int)((ClientRectangle.Width - (cols + 1) * space) / (float)cols);
            int height = (int)((ClientRectangle.Height - (rows + 1) * space) / (float)rows);
            
            for (int r = 0; r < rows; r++)
                for (int c = 0; c < cols; c++)
                    buttons[r, c].Bounds = new System.Drawing.Rectangle(
                        space * (c + 1) + width * c,
                        space * (r + 1) + height * r,
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
