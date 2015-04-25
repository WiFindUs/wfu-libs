using System;
using System.Net;
using WaveEngine.Common.Graphics;
using WaveEngine.Common.Math;
using WaveEngine.Components.Graphics3D;
using WaveEngine.Components.UI;
using WaveEngine.Framework;
using WaveEngine.Framework.Graphics;
using WaveEngine.Framework.Physics3D;
using WaveEngine.Materials;
using WiFindUs.Extensions;
using WiFindUs.Eye.Wave.Layers;

namespace WiFindUs.Eye.Wave.Markers
{
	public class DeviceMarker : EntityMarker<Device>, ILinkableMarker
	{
		private Transform3D coreTransform;
		private BasicMaterial matte;
		private Color colour = Color.White;
		private User user;

		/////////////////////////////////////////////////////////////////////
		// PROPERTIES
		/////////////////////////////////////////////////////////////////////

		public Vector3 LinkPoint
		{
			get { return coreTransform.Position; }
		}


		/////////////////////////////////////////////////////////////////////
		// CONSTRUCTORS
		/////////////////////////////////////////////////////////////////////

		public static DeviceMarker Create(Device device)
		{
			DeviceMarker marker = new DeviceMarker(device);

			Entity entity = new Entity()
				.AddComponent(new Transform3D())
				.AddComponent(marker)
				.AddComponent(marker.CylindricalCollider = new CylindricalCollider(12.0f, 4.0f, 6.0f))
				.AddComponent(new CylindricalColliderRenderer(2, 5))
				//spike
				.AddChild
				(
					new Entity("spike")
					.AddComponent(new Transform3D()
					{
						Position = new Vector3(0.0f, 5.0f, 0.0f),
						Rotation = new Vector3(180.0f.ToRadians(), 0f, 0f)
					})
					.AddComponent(new MaterialsMap(marker.matte = new BasicMaterial(MapScene.WhiteTexture)
					{
						LayerType = typeof(NonPremultipliedAlpha),
						LightingEnabled = true,
						AmbientLightColor = Color.White * 0.75f,
						Alpha = 0.5f
					}))
					.AddComponent(Model.CreateCone(8f, 6f, 8))
					.AddComponent(new ModelRenderer())
				)
				//core
				.AddChild
				(
					new Entity("core")
					.AddComponent(marker.coreTransform = new Transform3D()
					{
						LocalPosition = new Vector3(0.0f, 11.0f, 0.0f)
					})
					.AddComponent(new MaterialsMap(marker.matte))
					.AddComponent(Model.CreateSphere(3f, 4))
					.AddComponent(new ModelRenderer())
				)
				//selection
				.AddChild(SelectionRing.Create(device, 6.0f, 10, 7f).Owner);

			return marker;
		}

		internal DeviceMarker(Device d) : base(d) { }

		/////////////////////////////////////////////////////////////////////
		// PUBLIC METHODS
		/////////////////////////////////////////////////////////////////////

		public override string ToString()
		{
			return Entity.User != null
				? Entity.User.FullName + " (" + (Entity.User.Type == null || Entity.User.Type.Length == 0
					? "User ID #" + Entity.User.ID.ToString("X") : Entity.User.Type) + ")"
				: "Device ID #" + Entity.ID.ToString("X");
		}

		/////////////////////////////////////////////////////////////////////
		// PROTECTED METHODS
		/////////////////////////////////////////////////////////////////////

		protected override void Initialize()
		{
			base.Initialize();
			OnDeviceUserChanged(Entity);
			Entity.OnDeviceUserChanged += OnDeviceUserChanged;
			Entity.OnDeviceGPSEnabledChanged += OnDeviceGPSStateChanged;
			Entity.OnDeviceGPSHasFixChanged += OnDeviceGPSStateChanged;
			UpdateMarkerState();
			UpdateUI();
		}

		protected override void Update(TimeSpan gameTime)
		{
			base.Update(gameTime);
			if (!IsOwnerVisible)
				return;
			
			float secs = (float)gameTime.TotalSeconds;
			float targetAlpha = Entity.Active && (Entity.Selected || CameraTracking || CursorOver) ? 1.0f : 0.5f;
			matte.Alpha = matte.Alpha.Lerp(targetAlpha, secs * FADE_SPEED).Clamp(0.0f, 1.0f);
			matte.DiffuseColor = Color.Lerp(matte.DiffuseColor,
				!Entity.Active ? Color.DimGray : colour, secs * COLOUR_SPEED);
		}

		protected override void UpdateUI()
		{
			UIText.Text = Entity.User != null ? Entity.User.ShortName : "Device";
			UISubtext.Text = Entity.User != null
				? (Entity.User.Type == null || Entity.User.Type.Length == 0
					? "ID #" + Entity.User.ID.ToString("X") : Entity.User.Type)
				: " ID #" + Entity.ID.ToString("X");
			UpdateUserType();
		}

		/////////////////////////////////////////////////////////////////////
		// PRIVATE METHODS
		/////////////////////////////////////////////////////////////////////

		private void OnDeviceUserChanged(Device device)
		{
			if (user == device.User)
				return;
			if (user != null)
			{
				user.OnUserFirstNameChanged -= DeviceUserNameChanged;
				user.OnUserLastNameChanged -= DeviceUserNameChanged;
				user.OnUserMiddleNameChanged -= DeviceUserNameChanged;
				user.OnUserTypeChanged -= DeviceUserTypeChanged;
			}
			user = device.User;
			UpdateUI();
			if (user != null)
			{
				user.OnUserFirstNameChanged += DeviceUserNameChanged;
				user.OnUserLastNameChanged += DeviceUserNameChanged;
				user.OnUserMiddleNameChanged += DeviceUserNameChanged;
				user.OnUserTypeChanged += DeviceUserTypeChanged;
			}
		}

		private void DeviceUserNameChanged(User user)
		{
			UpdateUI();
		}

		private void DeviceUserTypeChanged(User user)
		{
			UpdateUI();
		}

		private void OnDeviceGPSStateChanged(Device device)
		{
			UpdateMarkerState();
		}

		private void UpdateUserType()
		{
			if (WFUApplication.Config == null
				|| Entity == null
				|| Entity.User == null
				|| Entity.User.Type == null
				|| Entity.User.Type.Length == 0)
			{
				UISubtext.Foreground = colour = Color.White;
				return;
			}

			UISubtext.Foreground = colour
				= ((System.Drawing.Color)WFUApplication.Config.Get("type_" + Entity.User.Type.ToLower().Trim() + ".colour",
				System.Drawing.Color.White)).Wave();

		}
	}
}
