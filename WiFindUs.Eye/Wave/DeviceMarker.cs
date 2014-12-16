using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WaveEngine.Common.Graphics;
using WaveEngine.Common.Math;
using WaveEngine.Components.Graphics3D;
using WaveEngine.Components.UI;
using WaveEngine.Framework;
using WaveEngine.Framework.Graphics;
using WaveEngine.Framework.Physics3D;
using WaveEngine.Materials;
using WiFindUs.Extensions;

namespace WiFindUs.Eye.Wave
{
    public class DeviceMarker : Behavior
    {
        private const float MAX_SPIN_RATE = 5.0f;

        [RequiredComponent]
        private Transform3D transform3D;
        private Device device;
        private MapScene scene;
        [RequiredComponent]
        private ModelRenderer renderer;

        /////////////////////////////////////////////////////////////////////
        // PROPERTIES
        /////////////////////////////////////////////////////////////////////

        public Device Device
        {
            get { return device; }
        }

        public float RotationSpeed
        {
            get
            {
                if (device.TimedOut)
                    return 0.0f;
                
                long age = device.UpdateAge;
                if (age == 0)
                    return MAX_SPIN_RATE;
                else if (age >= Device.TIMEOUT)
                    return 0.0f;
                else
                    return MAX_SPIN_RATE * (1.0f - (device.UpdateAge / (float)Device.TIMEOUT));
            }
        }

        /////////////////////////////////////////////////////////////////////
        // CONSTRUCTORS
        /////////////////////////////////////////////////////////////////////

        private DeviceMarker(Device device)
        {
            if (device == null)
                throw new ArgumentNullException("device", "Device cannot be null!");
            this.device = device;
            device.OnDeviceLocationChanged += OnDeviceLocationChanged;
            device.OnDeviceTimedOutChanged += OnDeviceTimedOutChanged;
            device.OnDeviceTypeChanged += OnDeviceTypeChanged;
        }

        public static Entity Create(Device device)
        {           
            return new Entity()
                .AddComponent(new Transform3D() { Rotation = new Vector3(180.0f.ToRadians(), 0f, 0f) })
                .AddComponent(new MaterialsMap(new BasicMaterial(Color.Red, DefaultLayers.Alpha) { LightingEnabled = true }))
                .AddComponent(Model.CreateCone(10f, 6f, 6))
                .AddComponent(new ModelRenderer())
                .AddComponent(new BoxCollider())
                .AddComponent(new DeviceMarker(device));

            
        }

        /////////////////////////////////////////////////////////////////////
        // PUBLIC METHODS
        /////////////////////////////////////////////////////////////////////

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

        /////////////////////////////////////////////////////////////////////
        // PROTECTED METHODS
        /////////////////////////////////////////////////////////////////////

        protected override void Initialize()
        {
            base.Initialize();
            scene = Owner.Scene as MapScene;
            UpdateDeviceState();
        }

        protected override void Update(TimeSpan gameTime)
        {
            if (Owner.IsVisible)
                transform3D.Rotation = new Vector3(
                    transform3D.Rotation.X,
                    transform3D.Rotation.Y + RotationSpeed * (float)gameTime.TotalSeconds,
                    transform3D.Rotation.Z);
        }

        /////////////////////////////////////////////////////////////////////
        // PRIVATE METHODS
        /////////////////////////////////////////////////////////////////////

        private void OnDeviceTypeChanged(Device obj)
        {

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
