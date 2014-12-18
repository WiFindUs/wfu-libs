using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
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
        private Device device;
        
        /////////////////////////////////////////////////////////////////////
        // PROPERTIES
        /////////////////////////////////////////////////////////////////////

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Device Device
        {
            get { return device; }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        protected override Color ImagePlaceholderColour
        {
            get
            {
                return device.User == null
                    ? base.ImagePlaceholderColour : WFUApplication.Config.Get("type_" + device.User.Type + ".colour", Color.Red);
            }
        }

        /////////////////////////////////////////////////////////////////////
        // CONSTRUCTORS
        /////////////////////////////////////////////////////////////////////

        public DeviceListItem(Device device)
            : base(device)
        {
            this.device = device;
            
            device.OnDeviceTypeChanged += device_OnDeviceTypeChanged;
            device.OnDeviceUserChanged += device_OnDeviceUserChanged;
            device.OnDeviceLocationChanged += device_OnDeviceLocationChanged;
            device.OnDeviceUpdated += device_OnDeviceUpdated;
            device.OnDeviceTimedOutChanged += device_OnDeviceTimedOutChanged;
            device.OnDeviceBatteryChanged += device_OnDeviceBatteryChanged;
            device.OnDeviceAssignedWaypointChanged += device_OnDeviceAssignedWaypointChanged;
        }

        private void device_OnDeviceAssignedWaypointChanged(Device obj)
        {
            Refresh();
        }

        private void device_OnDeviceBatteryChanged(Device obj)
        {
            Refresh();
        }

        private void device_OnDeviceTimedOutChanged(Device obj)
        {
            Refresh();
        }

        private void device_OnDeviceUpdated(Device obj)
        {
            Refresh();
        }

        private void device_OnDeviceLocationChanged(Device obj)
        {
            Refresh();
        }

        private void device_OnDeviceUserChanged(Device obj)
        {
            Refresh();
        }

        private void device_OnDeviceTypeChanged(Device obj)
        {
            Refresh();
        }
    }
}
