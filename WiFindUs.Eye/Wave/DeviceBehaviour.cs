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
            set
            {
                if (value == device)
                    return;
                device = value;
                if (device != null)
                {
                    UpdateVisible();
                    if (Owner != null && Owner.IsVisible)
                        UpdateLocation();
                }
            }
        }

        public DeviceBehaviour(Device device)
        {
            Device = device;
        }

        public void UpdateLocation()
        {
            if (scene == null || device == null || device.Location == null || !device.Location.HasLatLong)
                return;
            Vector3 pos = scene.LocationToVector(device.Location);
            pos.Y = 5f;
            transform3D.Position = pos;
        }

        public void UpdateVisible()
        {
            if (scene == null || device == null)
                return;
            long ms = (DateTime.UtcNow.ToUnixTimestamp() - device.Updated);
            Owner.IsVisible = device.LastUpdate < 3000;
        }

        protected override void Initialize()
        {
            base.Initialize();
            scene = Owner.Scene as MapScene;
            UpdateVisible();
            if (Owner.IsVisible)
                UpdateLocation();
        }

        protected override void Update(TimeSpan gameTime)
        {
            UpdateVisible();
            if (Owner.IsVisible)
                UpdateLocation();
        }
    }
}
