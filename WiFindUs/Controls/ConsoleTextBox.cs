using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using WiFindUs.Extensions;

namespace WiFindUs.Controls
{
	public class ConsoleTextBox : RichTextBox, IThemeable
	{
		private const int MAX_LINES = 4096;
		private Theme theme;
		private Debugger.Verbosity allowedVerbosities = Debugger.Verbosity.All ^ Debugger.Verbosity.Verbose;

		/////////////////////////////////////////////////////////////////////
		// PROPERTIES
		/////////////////////////////////////////////////////////////////////

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public Debugger.Verbosity AllowedVerbosities
		{
			get
			{
				return allowedVerbosities;
			}
			set
			{
				if (value == allowedVerbosities)
					return;
				allowedVerbosities = value;
				RegenerateFromHistory();
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
				BackColor = theme.ControlDarkColour;
				Font = theme.ConsoleFont;
				ForeColor = theme.TextLightColour;

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
#if DEBUG
			allowedVerbosities |= Debugger.Verbosity.Verbose;
#endif
		}

		/////////////////////////////////////////////////////////////////////
		// PUBLIC METHODS
		/////////////////////////////////////////////////////////////////////

		public virtual void OnThemeChanged()
		{

		}

		public void RegenerateFromHistory()
		{
			Clear();
			DebuggerLogItem[] items = Debugger.LogHistory;
			foreach (DebuggerLogItem item in items)
				if (allowedVerbosities.HasFlag(item.Verbosity))
					PrintLogItem(item);
			SendMessage(Handle, 277, (IntPtr)7, IntPtr.Zero);
		}

		/////////////////////////////////////////////////////////////////////
		// PROTECTED METHODS
		/////////////////////////////////////////////////////////////////////

		protected override void OnHandleCreated(EventArgs e)
		{
			base.OnHandleCreated(e);
			if (DesignMode || this.IsDesignMode())
				return;
			Debugger.OnDebugOutput += OnDebugOutput;
		}

		protected virtual void OnDebugOutput(DebuggerLogItem logItem)
		{
			if (!allowedVerbosities.HasFlag(logItem.Verbosity))
				return;

			if (InvokeRequired)
			{
				try
				{
					Invoke(new Action<DebuggerLogItem>(OnDebugOutput), new object[] { logItem });
				}
				catch (Exception) { }
				return;
			}

			PrintLogItem(logItem);
			SendMessage(Handle, 277, (IntPtr)7, IntPtr.Zero);
		}

		protected override void WndProc(ref Message m)
		{
			base.WndProc(ref m);
			if (DesignMode || this.IsDesignMode())
				return;
			HideCaret(this.Handle);
		}

		/////////////////////////////////////////////////////////////////////
		// PRIVATE METHODS
		/////////////////////////////////////////////////////////////////////

		private void PrintLogItem(DebuggerLogItem logItem)
		{
			//delete a big chunk of the text, keeping only the last third
			if (Lines.Length >= MAX_LINES)
			{
				Select(0, GetFirstCharIndexFromLine(2 * Lines.Length / 3));
				SelectedText = "";
			}

			SelectionStart = TextLength;
			SelectionLength = 0;
			SelectionColor = Debugger.Colours[logItem.Verbosity];
			AppendText(logItem.ToString() + "\n");
		}

		[DllImport("user32.dll", EntryPoint = "HideCaret")]
		private static extern long HideCaret(IntPtr hwnd);

		[DllImport("user32.dll", CharSet = CharSet.Auto)]
		private static extern int SendMessage(IntPtr hWnd, int wMsg, IntPtr wParam, IntPtr lParam);
	}
}
