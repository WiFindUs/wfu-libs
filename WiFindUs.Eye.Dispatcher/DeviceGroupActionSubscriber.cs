using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WiFindUs.Eye.Dispatcher
{
    public class DeviceGroupActionSubscriber : IActionSubscriber
    {
        private Device[] devices;

        public String ActionDescription
        {
            get { return String.Format("{0} devices", devices.Length); }
        }

        public DeviceGroupActionSubscriber(params Device[] devices)
        {
            if (devices == null)
                throw new ArgumentNullException("devices", "Devices cannot be null");
            if (devices.Length <= 0)
                throw new ArgumentOutOfRangeException("devices", "Devices array cannot be empty");
            this.devices = devices;
        }
        
        public bool ActionEnabled(uint index)
        {
            switch (index)
            {
                case 0: return true;
                //case 1: return true;
                //case 2: return true;
                case 8: return true;
            }
            return false;
        }

        public Image ActionImage(uint index)
        {
            return null;
        }

        public string ActionText(uint index)
        {
            switch (index)
            {
                case 0: return "Dispatch All";
                //case 1: return "Zoom To";
                //case 2: return "Track All";
                case 8: return "Cancel All";
            }
            return "";
        }

        public void ActionTriggered(uint index)
        {
            
        }
    }
}
