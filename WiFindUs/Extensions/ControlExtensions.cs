using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace WiFindUs.Extensions
{
    public static class ControlExtensions
    {
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
        }

        public static void IncrementThreadSafe(this ProgressBar progressBar, int value)
        {
            try
            {
                if (progressBar.InvokeRequired)
                    progressBar.Invoke(new Action<int>(progressBar.Increment), new object[] { value });
                else
                    progressBar.Increment(value);
            }
            catch (ObjectDisposedException)
            {
                return;
            }
        }
    }
}
