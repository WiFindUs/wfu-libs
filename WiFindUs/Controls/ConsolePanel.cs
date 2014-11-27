using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Runtime.InteropServices;

namespace WiFindUs.Controls
{
	public class ConsolePanel : Panel, IThemeable
	{
        private Theme theme;
        private RichTextBox console;
		private TextBox input;

		private const int MAX_CONSOLE_LINES = 1024;
		private readonly Color[] colors = new Color[6];

        /////////////////////////////////////////////////////////////////////
        // PROPERTIES
        /////////////////////////////////////////////////////////////////////

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
                BackColor = console.BackColor = input.BackColor = theme.ControlDarkColour;
                console.Font = input.Font = theme.ConsoleFont;
                Refresh();
            }
        }

        /////////////////////////////////////////////////////////////////////
        // CONSTRUCTORS
        /////////////////////////////////////////////////////////////////////

		public ConsolePanel()
		{
			if (DesignMode)
				return;
			
			SuspendLayout();

			//input box
			input = new TextBox();
			input.KeyPress += new KeyPressEventHandler(input_KeyPress);
			input.Location = new Point(0, ClientRectangle.Height - input.Height);
			input.Width = ClientRectangle.Width;
			input.Dock = DockStyle.Bottom;
			Controls.Add(input);

			//console panel
			console = new RichTextBox();
			console.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom | AnchorStyles.Top;
			console.Margin = new Padding(0);
			console.BorderStyle = BorderStyle.None;
			console.ScrollBars = RichTextBoxScrollBars.Vertical;
			console.ReadOnly = true;
			console.Bounds = new Rectangle(0,0,ClientRectangle.Width,ClientRectangle.Height - input.Height - 1);
			Controls.Add(console);

			//config
			colors[0] = WFUApplication.Config.Get("console.verbose", Color.Gray);
			colors[1] = WFUApplication.Config.Get("console.information", Color.White);
			colors[2] = WFUApplication.Config.Get("console.warning", Color.Yellow);
			colors[3] = WFUApplication.Config.Get("console.error", Color.Red);
			colors[4] = WFUApplication.Config.Get("console.exception", Color.Red);
			colors[5] = input.ForeColor
					  = WFUApplication.Config.Get("console.prompts", Color.Green);

			ResumeLayout(false);
			PerformLayout();
	
			//events
			Debugger.OnDebugOutput += OnDebugOutput;
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
			;
		}

		protected virtual void OnDebugOutput(Debugger.Verbosity level, string prefix, string text)
		{
			if (InvokeRequired)
			{
				try
				{
					Invoke(new Debugger.LogDelegate(OnDebugOutput), new object[] { level, prefix, text });
				}
				catch (Exception) { }
				return;
			}

			if (console.Lines.Length >= MAX_CONSOLE_LINES)
			{
				string[] keepLines = new string[MAX_CONSOLE_LINES / 2 + 1];
				console.Lines.CopyTo(keepLines, MAX_CONSOLE_LINES / 2);
				console.Lines = keepLines;
			}

			console.SelectionColor = colors[(int)level];
			console.AppendText(prefix + text + "\n");
			SendMessage(console.Handle, 277, (IntPtr)7, IntPtr.Zero);
		}

        /////////////////////////////////////////////////////////////////////
        // PRIVATE METHODS
        /////////////////////////////////////////////////////////////////////

		[DllImport("user32.dll", CharSet = CharSet.Auto)]
		private static extern int SendMessage(IntPtr hWnd, int wMsg, IntPtr wParam, IntPtr lParam);
	}
}
