using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using WiFindUs.Controls;

namespace WiFindUs.Forms
{
    public class ConsoleForm : BaseForm
	{
		private ConsolePanel console = null;

        /////////////////////////////////////////////////////////////////////
        // CONSTRUCTORS
        /////////////////////////////////////////////////////////////////////

		public ConsoleForm()
		{
            if (IsDesignMode)
				return;

            ShowInTaskbar = false;
			Text = WFUApplication.Name + " :: Console";

			Controls.Add(console = new ConsolePanel()
			{
				Dock = DockStyle.Fill,
			});
		}

        /////////////////////////////////////////////////////////////////////
        // PROTECTED METHODS
        /////////////////////////////////////////////////////////////////////

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
