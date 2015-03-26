﻿using System;
using System.ComponentModel;
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
				{
					toggles[i].Font = value.ConsoleFont;
					toggles[i].ForeColor = toggles[i].Checked ? Theme.ControlDarkColour : Theme.TextDarkColour;
				}

			}
		}

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
