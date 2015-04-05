using System;
using System.ComponentModel;
using System.Windows.Forms;
using WiFindUs.Themes;

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
		public Debugger.Verbosity AllowedVerbosities
		{
			get
			{
				Debugger.Verbosity allowedVerbosities = Debugger.Verbosity.None;
				for (int i = 0; i < toggles.Length; i++)
					if (toggles[i].Checked && toggles[i].Tag != null)
						allowedVerbosities |= (Debugger.Verbosity)toggles[i].Tag;
				return allowedVerbosities;
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
		// PUBLIC METHODS
		/////////////////////////////////////////////////////////////////////

		public override void ApplyTheme(ITheme theme)
		{
			if (theme == null)
				return;

			BackColor = input.BackColor = theme.Background.Dark.Colour;
			Font = input.Font = theme.Monospaced.Normal.Regular;
			ForeColor = input.ForeColor = theme.Foreground.Light.Colour;
			for (int i = 0; i < toggles.Length; i++)
			{
				toggles[i].Font = Font;
				toggles[i].ForeColor = toggles[i].Checked ? theme.Background.Dark.Colour : theme.Foreground.Dark.Colour;
			}
		}

		public void RegenerateFromHistory()
		{
			console.RegenerateFromHistory();
		}

		/////////////////////////////////////////////////////////////////////
		// PROTECTED METHODS
		/////////////////////////////////////////////////////////////////////

		protected override void OnHandleCreated(EventArgs e)
		{
			base.OnHandleCreated(e);
			if (IsDesignMode)
				return;
			for (int i = 0; i < toggles.Length; i++)
			{
				toggles[i].Tag = verbosities[i];
				toggles[i].FlatAppearance.CheckedBackColor = Debugger.Colours[verbosities[i]];
				toggles[i].Checked = console.AllowedVerbosities.HasFlag(verbosities[i]);
				toggles[i].CheckedChanged += ToggleCheckedChanged;
				toggles[i].Enabled = Debugger.AllowedVerbosities.HasFlag(verbosities[i]);
			}
		}

		protected override void OnResize(EventArgs e)
		{
			base.OnResize(e);

			input.Width = controls.ClientSize.Width - input.Left;
			console.Height = controls.Top - 1;
		}

		/////////////////////////////////////////////////////////////////////
		// PRIVATE METHODS
		/////////////////////////////////////////////////////////////////////

		private void ToggleCheckedChanged(object sender, EventArgs e)
		{
			CheckBox toggle = sender as CheckBox;
			if (toggle == null || toggle.Tag == null)
				return;

			console.AllowedVerbosities = AllowedVerbosities;
			toggle.ForeColor = toggle.Checked ? Theme.Current.Background.Dark.Colour : Theme.Current.Foreground.Dark.Colour;
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
