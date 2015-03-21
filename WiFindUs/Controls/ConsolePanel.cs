using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WiFindUs.Controls
{
    public partial class ConsolePanel : ThemedUserControl
    {
        private CheckBox[] toggles = new CheckBox[6];
        private Debugger.Verbosity[] verbosities = new Debugger.Verbosity[6];
        
        /////////////////////////////////////////////////////////////////////
        // PROPERTIES
        /////////////////////////////////////////////////////////////////////

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

                BackColor = input.BackColor = value.ControlDarkColour;
                Font = input.Font = value.ConsoleFont;
                ForeColor = input.ForeColor = value.TextLightColour;
                for (int i = 0; i < toggles.Length; i++)
                    toggles[i].Font = value.ConsoleFont;
                
            }
        }

        /////////////////////////////////////////////////////////////////////
        // CONSTRUCTORS
        /////////////////////////////////////////////////////////////////////
        
        public ConsolePanel()
        {
            InitializeComponent();

            toggles[0] = verboseToggle;
            toggles[1] = infoToggle;
            toggles[2] = warningToggle;
            toggles[3] = errorToggle;
            toggles[4] = exceptionToggle;
            toggles[5] = consoleToggle;

            verbosities[0] = Debugger.Verbosity.Verbose;
            verbosities[1] = Debugger.Verbosity.Information;
            verbosities[2] = Debugger.Verbosity.Warning;
            verbosities[3] = Debugger.Verbosity.Error;
            verbosities[4] = Debugger.Verbosity.Exception;
            verbosities[5] = Debugger.Verbosity.Console;

            input.AutoSize = false;
            input.Height = controls.Height - 2;

#if !DEBUG
            errorToggle.Visible = false;
            exceptionToggle.Visible = false;
#endif
        }

        /////////////////////////////////////////////////////////////////////
        // PROTECTED METHODS
        /////////////////////////////////////////////////////////////////////

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);

            for (int i = 0; i < toggles.Length; i++)
            {
                toggles[i].Tag = verbosities[i];
                toggles[i].FlatAppearance.CheckedBackColor = ConsoleTextBox.Colours[verbosities[i]];
                toggles[i].Checked = console.AllowedVerbosities.HasFlag(verbosities[i]);
                toggles[i].CheckedChanged += ToggleCheckedChanged;
                if (!IsDesignMode)
                    toggles[i].ForeColor = toggles[i].Checked ? Theme.ControlDarkColour : Theme.TextDarkColour;
                if (!Debugger.AllowedVerbosities.HasFlag(verbosities[i]))
                    toggles[i].Enabled = false;
            }
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            input.Width = controls.ClientSize.Width - input.Left;
            console.Height = controls.Top - 1;
        }

        protected override void OnGotFocus(EventArgs e)
        {
            base.OnGotFocus(e);
            input.Focus();
        }

        /////////////////////////////////////////////////////////////////////
        // PRIVATE METHODS
        /////////////////////////////////////////////////////////////////////

        private void ToggleCheckedChanged(object sender, EventArgs e)
        {
            CheckBox toggle = sender as CheckBox;
            if (toggle == null || toggle.Tag == null)
                return;

            Debugger.Verbosity allowedVerbosities = Debugger.Verbosity.None;
            for (int i = 0; i < toggles.Length; i++)
                if (toggles[i].Checked)
                    allowedVerbosities |= (Debugger.Verbosity)toggles[i].Tag;
            console.AllowedVerbosities = allowedVerbosities;

            toggle.ForeColor = toggle.Checked ? Theme.ControlDarkColour : Theme.TextDarkColour;
        }

        private void InputKeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '\r' || e.KeyChar == '\n')
            {
                string command = input.Text.Trim();

                if (command.Length > 0)
                {
                    if (command.ToLower().CompareTo("clear") == 0)
                        console.Clear();
                    else
                        Debugger.C("> " + command);
                    input.Text = "";
                }
                e.Handled = true;
            }
        }
    }
}
