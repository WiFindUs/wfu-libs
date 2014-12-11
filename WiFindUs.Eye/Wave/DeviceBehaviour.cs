using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WaveEngine.Common.Math;
using WaveEngine.Components.Graphics3D;
using WaveEngine.Framework;
using WaveEngine.Framework.Graphics;
using WiFindUs.Extensions;

namespace WiFindUs.Eye.Wave
{
    public class DeviceBehaviour : Behavior
    {
        [RequiredComponent]
        private Transform3D transform3D;
        private Device device;
        private MapScene scene;
        [RequiredComponent]
        private ModelRenderer renderer;

        public Device Device
        {
            get { return device; }
        }

        public DeviceBehaviour(Device device)
        {
            if (device == null)
                throw new ArgumentNullException("device", "Device cannot be null!");
            this.device = device;
            device.OnDeviceLocationChanged += OnDeviceLocationChanged;
            device.OnDeviceTimedOutChanged += OnDeviceTimedOutChanged;
        }

        public void UpdateDeviceState()
        {
            if (scene == null || device == null)
                return;
            bool vis = !device.TimedOut && device.Location.HasLatLong;
            if (vis != Owner.IsVisible)
                Owner.IsVisible = vis;
            if (vis)
            {
                Vector3 pos = scene.LocationToVector(device.Location);
                pos.Y = 5f;
                transform3D.Position = pos;
            }
        }

        protected override void Initialize()
        {
            base.Initialize();
            scene = Owner.Scene as MapScene;
            UpdateDeviceState();
        }

        protected override void Update(TimeSpan gameTime)
        {
            //UpdateVisible();
            //if (Owner.IsVisible)
            //    UpdateLocation();
        }

        private void OnDeviceLocationChanged(Device obj)
        {
            UpdateDeviceState();
        }

        private void OnDeviceTimedOutChanged(Device obj)
        {
            UpdateDeviceState();
        }
    }
}
