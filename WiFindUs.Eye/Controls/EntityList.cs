using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WiFindUs.Controls;

namespace WiFindUs.Eye.Controls
{
    public class EntityList : ThemedFlowLayoutPanel
    {
        public EntityList()
        {
            SuspendLayout();
            WrapContents = false;
            FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            ResumeLayout(true);
        }

        protected override void OnControlAdded(ControlEventArgs e)
        {
            SuspendLayout();
            base.OnControlAdded(e);
            EntityListChild elc = e.Control as EntityListChild;
            if (elc == null)
                return;
            elc.Width = ClientRectangle.Width - 1;
            ResumeLayout(true);
        }
    }
}
