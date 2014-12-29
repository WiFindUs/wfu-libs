using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Runtime.InteropServices;
using System.ComponentModel;

namespace WiFindUs.Controls
{
	public class ConsolePanel : Panel, IThemeable
	{
        private Theme theme;
        private ConsoleTextBox console;
		private TextBox input;

        /////////////////////////////////////////////////////////////////////
        // PROPERTIES
        /////////////////////////////////////////////////////////////////////

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
                BackColor = input.BackColor = theme.ControlDarkColour;
                Font = input.Font = theme.ConsoleFont;
                ForeColor = input.ForeColor = theme.TextLightColour;
            }
        }

        /////////////////////////////////////////////////////////////////////
        // CONSTRUCTORS
        /////////////////////////////////////////////////////////////////////

		public ConsolePanel()
		{
            if (DesignMode)
            {
                theme = WFUApplication.Theme;
                return;
            }
			
			SuspendLayout();

			//input box
			input = new TextBox();
			input.KeyPress += new KeyPressEventHandler(input_KeyPress);
			input.Location = new Point(0, ClientRectangle.Height - input.Height);
			input.Width = ClientRectangle.Width;
			input.Dock = DockStyle.Bottom;
			Controls.Add(input);

			//console panel
			console = new ConsoleTextBox();
			console.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom | AnchorStyles.Top;
			console.Bounds = new Rectangle(0,0,ClientRectangle.Width,ClientRectangle.Height - input.Height - 1);
			Controls.Add(console);

			ResumeLayout(false);
			PerformLayout();
		}

        /////////////////////////////////////////////////////////////////////
        // PROTECTED METHODS
        /////////////////////////////////////////////////////////////////////

		protected override void OnGotFocus(EventArgs e)
		{
			base.OnGotFocus(e);
			input.Focus();
		}

		protected virtual void input_KeyPress(object sender, KeyPressEventArgs e)
		{
			if (e.KeyChar == '\r' || e.KeyChar == '\n')
            {
                string command = input.Text.Trim();
                if (command.Length > 0)
                {
                    input.Text = "";
                    Debugger.C(">" + command);
                }

                command = command.ToLower();
                if (command.CompareTo("clear") == 0)
                    console.Clear();
                e.Handled = true;
            }
		}
	}
}
