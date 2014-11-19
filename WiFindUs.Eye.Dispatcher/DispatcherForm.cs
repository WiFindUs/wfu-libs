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
    }
}
