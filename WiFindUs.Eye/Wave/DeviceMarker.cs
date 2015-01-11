using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    public class DeviceMarker : EntityMarker
    {
        private const float MAX_SPIN_RATE = 5.0f;
        private Device device;
        [RequiredComponent]
        private Transform3D transform3D;
        [RequiredComponent]
        private MaterialsMap materialsMap;

        /////////////////////////////////////////////////////////////////////
        // PROPERTIES
        /////////////////////////////////////////////////////////////////////

        public Device Device
        {
            get { return device; }
        }

        public override Material CurrentMaterial
        {
            get
            {
                if (device == null || device.User == null)
                    return base.CurrentMaterial;
                return UserTypeMaterial(device.User);
            }
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

        public override Transform3D Transform3D
        {
            get { return transform3D; }
        }

        public override MaterialsMap MaterialsMap
        {
            get { return materialsMap; }
        }

        /////////////////////////////////////////////////////////////////////
        // CONSTRUCTORS
        /////////////////////////////////////////////////////////////////////

        private DeviceMarker(Device device)
            : base (device)
        {
            this.device = device;
        }

        public static Entity Create(Device device)
        {
            return new Entity() { IsActive = false, IsVisible = false }
                .AddComponent(new Transform3D() { Rotation = new Vector3(180.0f.ToRadians(), 0f, 0f) })
                .AddComponent(new MaterialsMap(PlaceHolderMaterial))
                .AddComponent(Model.CreateCone(10f, 6f, 6))
                .AddComponent(new ModelRenderer())
                .AddComponent(new BoxCollider())
                .AddComponent(new DeviceMarker(device))
                .AddChild(new Entity() { IsActive = false, IsVisible = false }
                    .AddComponent(new Transform3D())
                    .AddComponent(new MaterialsMap(SelectedMaterial))
                    .AddComponent(Model.CreateTorus(16, 1, 10))
                    .AddComponent(new ModelRenderer())
                );
        }

        /////////////////////////////////////////////////////////////////////
        // PROTECTED METHODS
        /////////////////////////////////////////////////////////////////////

        protected override void Initialize()
        {
            base.Initialize();

            device.OnDeviceLocationChanged += OnDeviceLocationChanged;
            device.OnDeviceTimedOutChanged += OnDeviceTimedOutChanged;
            device.OnDeviceUserChanged += OnDeviceUserChanged;

            UpdateMarkerState();
        }

        protected override void Update(TimeSpan gameTime)
        {
            if (Owner.IsVisible)
            {
                Transform3D.Rotation = new Vector3(
                    Transform3D.Rotation.X,
                    Transform3D.Rotation.Y + RotationSpeed * (float)gameTime.TotalSeconds,
                    Transform3D.Rotation.Z);
            }
        }

        /////////////////////////////////////////////////////////////////////
        // PRIVATE METHODS
        /////////////////////////////////////////////////////////////////////

        private void OnDeviceLocationChanged(Device device)
        {
            UpdateMarkerState();
        }

        private void OnDeviceTimedOutChanged(Device device)
        {
            UpdateMarkerState();
        }

        private void OnDeviceUserChanged(Device device)
        {
            UpdateMarkerState();
        }
    }
}
