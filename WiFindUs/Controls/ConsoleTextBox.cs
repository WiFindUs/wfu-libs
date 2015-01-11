﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WiFindUs.Controls
{
    public class ConsoleTextBox : RichTextBox, IThemeable
    {
        private static readonly int MAX_CONSOLE_LINES = 16384;
        private int mouseDown = 0;
        
        private readonly Color[] colors = new Color[6];
        private Theme theme;

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
                BackColor = theme.ControlDarkColour;
                Font = theme.ConsoleFont;
                ForeColor = theme.TextLightColour;

                colors[0] = theme.TextDarkColour; //verbose
                colors[1] = theme.TextLightColour; //info
                colors[2] = theme.WarningColour; //warning
                colors[3] = theme.ErrorColour; //error
                colors[4] = theme.ErrorColour; //exception
                colors[5] = theme.HighlightLightColour; //prompts/console

                OnThemeChanged();
            }
        }

        /////////////////////////////////////////////////////////////////////
        // CONSTRUCTORS
        /////////////////////////////////////////////////////////////////////

        public ConsoleTextBox()
	    {
		    ReadOnly = true;
		    BorderStyle = BorderStyle.None;
            ScrollBars = RichTextBoxScrollBars.Vertical;
            Margin = new Padding(0);
		    TabStop = false;

            //events
            Debugger.OnDebugOutput += OnDebugOutput;
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

        protected override void DefWndProc(ref Message m)
        {
            base.DefWndProc(ref m);
            /*
            if (m.Msg == 277)
            {



                eventFired++;

                if (eventFired == 2)
                {

                    thumbDown(this, System.EventArgs.Empty);

                    eventFired = 0;

                }



            }



            if (m.Msg == 277 && (int)m.WParam == 8)

                thumbUp(this, System.EventArgs.Empty);
             * */
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

            if (Lines.Length >= MAX_CONSOLE_LINES)
                Clear();

            SelectionStart = TextLength;
            SelectionLength = 0;
            SelectionColor = colors[(int)level];
            AppendText(prefix + text + "\n");
            if (mouseDown == 0)
                SendMessage(Handle, 277, (IntPtr)7, IntPtr.Zero);
        }

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);
            HideCaret(this.Handle);
        }

        /////////////////////////////////////////////////////////////////////
        // PRIVATE METHODS
        /////////////////////////////////////////////////////////////////////

        [DllImport("user32.dll", EntryPoint = "HideCaret")]
        private static extern long HideCaret(IntPtr hwnd);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern int SendMessage(IntPtr hWnd, int wMsg, IntPtr wParam, IntPtr lParam);
    }
}
