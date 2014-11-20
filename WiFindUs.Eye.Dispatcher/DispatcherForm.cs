using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WiFindUs.Eye.Dispatcher
{
    public partial class DispatcherForm : MainForm
    {
        public DispatcherForm()
        {
            InitializeComponent();
            if (DesignMode)
                return;
        }

        protected override void OnFirstShown(EventArgs e)
        {
            base.OnFirstShown(e);
            
            Screen primary = Screen.PrimaryScreen;
            Rectangle primaryBounds = primary.WorkingArea;
            if (WFUApplication.UsesConsoleForm)
            {

                Bounds = new Rectangle(primaryBounds.Left, primaryBounds.Top, primaryBounds.Width, 3 * primaryBounds.Height / 4);
                Console.Bounds = new Rectangle(Bounds.Left, Bounds.Bottom + 3, Bounds.Width, primaryBounds.Height - 6 - Bounds.Height);
            }
        }
    }
}
