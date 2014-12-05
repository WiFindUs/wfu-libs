using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WaveEngine.Common.Math;
using WaveEngine.Framework;
using WaveEngine.Framework.Graphics;

namespace WiFindUs.Eye.Wave
{
    public class DeviceBehaviour : Behavior
    {
        [RequiredComponent]
        private Transform3D transform3D;
        private Device device;
        private MapScene scene;

        public DeviceBehaviour(Device device)
        {
            this.device = device;
        }

        private void device_OnLocationChanged(Device sender)
        {
            if (sender.Location == null)
            {
                Debugger.E(String.Format("Device[{0}] pushed a null location", sender.ID));
                return;
            }
            if (!sender.Location.HasLatLong)
                return;
            Vector3 pos = scene.LocationToVector(sender.Location);
            Debugger.W(pos.ToString());
            pos.Y = 50f;
            transform3D.Position = pos;
        }

        protected override void Initialize()
        {
            base.Initialize();
            scene = Owner.Scene as MapScene;
            device_OnLocationChanged(device);
            device.OnDeviceLocationChanged += device_OnLocationChanged;
        }

        protected override void Update(TimeSpan gameTime)
        {

        }
    }
}
