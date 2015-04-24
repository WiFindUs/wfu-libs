using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace WiFindUs.Extensions
{
	public static class ControlExtensions
	{
		public static bool IsDesignMode(this Component comp)
		{
			return (comp.Site != null && comp.Site.DesignMode)
				|| LicenseManager.UsageMode == LicenseUsageMode.Designtime
				|| AppDomain.CurrentDomain.FriendlyName.Equals("DefaultDomain");
		}

		public static void RefreshThreadSafe(this Control control)
		{
			try
			{
				if (control.InvokeRequired)
					control.Invoke(new Action(control.Refresh));
				else
					control.Refresh();
			}
			catch (ObjectDisposedException)
			{
				return;
			}
			catch (InvalidAsynchronousStateException)
			{
				return;
			}
		}

		public static void InvalidateThreadSafe(this Control control)
		{
			try
			{
				if (control.InvokeRequired)
					control.Invoke(new Action(control.Invalidate));
				else
					control.Invalidate();
			}
			catch (ObjectDisposedException)
			{
				return;
			}
			catch (InvalidAsynchronousStateException)
			{
				return;
			}
		}

		public static void SetText(this Control control, string text)
		{
			try
			{
				if (control.InvokeRequired)
					control.Invoke(new Action<Control, string>(SetText), new object[] { control, text });
				else
					control.Text = text;
			}
			catch (ObjectDisposedException)
			{
				return;
			}
			catch (InvalidAsynchronousStateException)
			{
				return;
			}
		}

		public static void IncrementThreadSafe(this ProgressBar progressBar, int value)
		{
			try
			{
				if (progressBar.InvokeRequired)
					progressBar.Invoke(new Action<int>(progressBar.Increment), value);
				else
					progressBar.Increment(value);
			}
			catch (ObjectDisposedException)
			{
				return;
			}
		}

		public static void RecurseControls(this Control root, Action<Control> action)
		{
			foreach (Control child in root.Controls)
			{
				if (child == root)
					continue;
				action(child);
				RecurseControls(child, action);
			}
		}

		public static void SuspendAllLayout(this Control root)
		{
			root.SuspendLayout();
			root.RecurseControls(control => control.SuspendLayout());
		}

		public static void ResumeAllLayout(this Control root, bool performLayout = true)
		{
			root.RecurseControls(control => control.ResumeLayout(performLayout));
			root.ResumeLayout(performLayout);
		}

		public static void HidePanel(this SplitContainer container, int panel)
		{
			panel = panel > 2 ? 2 : (panel < 1 ? 1 : panel);
			if (panel == 1)
			{
				container.Panel1Collapsed = true;
				container.Panel1.Hide();
			}
			else
			{
				container.Panel2Collapsed = true;
				container.Panel2.Hide();
			}
		}

		public static void ShowPanel(this SplitContainer container, int panel)
		{
			panel = panel > 2 ? 2 : (panel < 1 ? 1 : panel);
			if (panel == 1)
			{
				container.Panel1.Show();
				container.Panel1Collapsed = false;
			}
			else
			{
				container.Panel2.Show();
				container.Panel2Collapsed = false;
			}
		}
	}
}
