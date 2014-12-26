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
        private MaterialsMap materialsMap;

        private static Material placeHolderMaterial, selectedMaterial;
        private static Dictionary<string, Material> userTypeColours = new Dictionary<string, Material>();

        public static Material PlaceHolderMaterial
        {
            get
            {
                if (placeHolderMaterial == null)
                    placeHolderMaterial = new BasicMaterial(Color.Gray) { LightingEnabled = true };
                return placeHolderMaterial;
            }
        }

        public static Material SelectedMaterial
        {
            get
            {
                if (selectedMaterial == null)
                    selectedMaterial = new BasicMaterial(Color.Yellow) { LightingEnabled = true };
                return selectedMaterial;
            }
        }

        protected Material CurrentMaterial
        {
            get
            {
                if (device == null)
                    return PlaceHolderMaterial;
                if (device.Selected)
                    return SelectedMaterial;
                if (device.User == null)
                    return PlaceHolderMaterial;
                string type = device.User.Type;
                if (type == null || (type = type.Trim().ToLower()).Length == 0)
                    return PlaceHolderMaterial;

                Material material = PlaceHolderMaterial;
                if (!userTypeColours.TryGetValue(type, out material))
                {
                    System.Drawing.Color col
                        = WFUApplication.Config.Get("type_" + device.User.Type + ".colour", System.Drawing.Color.Gray);
                    userTypeColours[type] = material
                        = new BasicMaterial(new Color(col.R, col.G, col.B, col.A)) { LightingEnabled = true };
                }
                return material;               
            }
        }

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
        }

        public static Entity Create(Device device)
        {           
            return new Entity()
                .AddComponent(new Transform3D()
                {
                    Rotation = new Vector3(180.0f.ToRadians(), 0f, 0f),
                    Scale = device.Selected ? new Vector3(2.0f, 2.0f, 2.0f) : new Vector3(1.0f, 1.0f, 1.0f)
                })
                .AddComponent(new MaterialsMap(PlaceHolderMaterial))
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
                pos.Y = device.Selected ? 10f : 5f;
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

            device.OnDeviceLocationChanged += OnDeviceLocationChanged;
            device.OnDeviceTimedOutChanged += OnDeviceTimedOutChanged;
            device.OnDeviceUserChanged += OnDeviceUserChanged;
            device.SelectedChanged += DeviceSelectedChanged;

            DeviceSelectedChanged(device);
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

        private void OnDeviceLocationChanged(Device device)
        {
            UpdateDeviceState();
        }

        private void OnDeviceTimedOutChanged(Device device)
        {
            UpdateDeviceState();
        }

        private void DeviceSelectedChanged(ISelectableEntity device)
        {
            transform3D.Position = new Vector3(transform3D.Position.X, device.Selected ? 10f : 5f, transform3D.Position.Z);
            transform3D.Scale = device.Selected ? new Vector3(2.0f, 2.0f, 2.0f) : new Vector3(1.0f, 1.0f, 1.0f);
            materialsMap.DefaultMaterial = CurrentMaterial;
        }

        private void OnDeviceUserChanged(Device device)
        {
            materialsMap.DefaultMaterial = CurrentMaterial;
        }
    }
}
