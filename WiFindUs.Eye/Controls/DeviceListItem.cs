using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WiFindUs.Controls;
using WiFindUs.Extensions;

namespace WiFindUs.Eye.Controls
{
    public class DeviceListItem : EntityListItem
    {
        /////////////////////////////////////////////////////////////////////
        // PROPERTIES
        /////////////////////////////////////////////////////////////////////

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Device Device
        {
            get { return Entity as Device; }
        }

        /////////////////////////////////////////////////////////////////////
        // CONSTRUCTORS
        /////////////////////////////////////////////////////////////////////

        public DeviceListItem(Device device)
            : base(device)
        {

        }
    }
}
