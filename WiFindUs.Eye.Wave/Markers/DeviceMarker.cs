using System;
using System.Net;
using WaveEngine.Common.Graphics;
using WaveEngine.Common.Math;
using WaveEngine.Components.Graphics3D;
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
		private BasicMaterial spikeMat, coreMat;
		private Color colour = Color.White;
		private float fader = 0.0f;

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

		public static Entity Create(Device device)
		{
			DeviceMarker marker = new DeviceMarker(device);

			return new Entity()
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
					.AddComponent(new MaterialsMap(marker.spikeMat = new BasicMaterial(MapScene.WhiteTexture)
					{
						LayerType = typeof(NonPremultipliedAlpha),
						LightingEnabled = true,
						AmbientLightColor = Color.White * 0.75f,
						Alpha = 0.75f
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
					.AddComponent(new MaterialsMap(marker.coreMat = new BasicMaterial(MapScene.WhiteTexture)
					{
						LayerType = typeof(NonPremultipliedAlpha),
						LightingEnabled = true,
						AmbientLightColor = Color.White * 0.75f,
						Alpha = 0.75f
					}))
					.AddComponent(Model.CreateSphere(3f, 4))
					.AddComponent(new ModelRenderer())
				)
				//selection
				.AddChild(SelectionRing.Create(device, 6.0f, 10, 7f));
		}

		internal DeviceMarker(Device d) : base(d) { }

		/////////////////////////////////////////////////////////////////////
		// PROTECTED METHODS
		/////////////////////////////////////////////////////////////////////

		protected override void Initialize()
		{
			base.Initialize();
			entity.OnDeviceUserChanged += OnDeviceUserChanged;
			entity.OnDeviceGPSEnabledChanged += OnDeviceGPSStateChanged;
			entity.OnDeviceGPSHasFixChanged += OnDeviceGPSStateChanged;
			UpdateMarkerState();
		}

		protected override void Update(TimeSpan gameTime)
		{
			base.Update(gameTime);
			if (!Owner.IsVisible)
				return;
			
			float secs = (float)gameTime.TotalSeconds;
			float targetAlpha = !Entity.Active ? 0.5f : (Entity.Selected || MapScene.Camera.TrackingTarget == this.Entity ? 1.0f : 0.75f);
			spikeMat.Alpha = spikeMat.Alpha.Lerp(targetAlpha, secs * FADE_SPEED);
			coreMat.Alpha = spikeMat.Alpha;
			spikeMat.DiffuseColor = Color.Lerp(spikeMat.DiffuseColor,
				!Entity.Active ? Color.DimGray : colour, secs * COLOUR_SPEED);
			if (Entity.Active)
				coreMat.DiffuseColor = Color.White.Coserp(Color.Cyan, fader += secs);
		}

		/////////////////////////////////////////////////////////////////////
		// PRIVATE METHODS
		/////////////////////////////////////////////////////////////////////

		private void OnDeviceUserChanged(Device device)
		{
			UpdateMarkerState();
		}

		private void OnDeviceGPSStateChanged(Device device)
		{
			UpdateMarkerState();
		}
	}
}
