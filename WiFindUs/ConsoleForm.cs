using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace WiFindUs
{
	public class ConsoleForm : Form
	{
		private ConsolePanel console = null;

		public ConsoleForm()
		{
			if (DesignMode)
				return;

			SetStyle(ControlStyles.DoubleBuffer
			| ControlStyles.AllPaintingInWmPaint
			| ControlStyles.ResizeRedraw
			| ControlStyles.OptimizedDoubleBuffer, true);

			Text = WFUApplication.Name + " :: Console";

			Controls.Add(console = new ConsolePanel()
			{
				Dock = DockStyle.Fill,
			});
		}

		protected override void OnFormClosing(FormClosingEventArgs e)
		{
			if (e.Cancel = (e.CloseReason == CloseReason.UserClosing))
				Hide();
			else
				base.OnFormClosing(e);
		}

		protected override void OnShown(EventArgs e)
		{
			base.OnShown(e);
			console.Focus();
		}
	}
}
