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
using WiFindUs.Eye.Wave.Extensions;

namespace WiFindUs.Eye.Wave.Markers
{
	public class DeviceMarker : EntityMarker<Device>, ILinkableMarker
	{
		private Entity spike;
		private BasicMaterial spikeMat;
		
		/////////////////////////////////////////////////////////////////////
		// PROPERTIES
		/////////////////////////////////////////////////////////////////////

		public Vector3 LinkPointPrimary
		{
			get
			{
				return new Vector3(
					this.Transform3D.Position.X,
					this.Transform3D.Position.Y + 7.0f * Transform3D.Scale.Y,
					this.Transform3D.Position.Z
					);
			}
		}

		public Vector3 LinkPointSecondary
		{
			get { return LinkPointPrimary; }
		}

		protected override bool VisibilityOverride
		{
			get
			{
				return entity.GPSEnabled.GetValueOrDefault()
					&& entity.GPSHasFix.GetValueOrDefault()
					&& base.VisibilityOverride;
			}
		}

		/////////////////////////////////////////////////////////////////////
		// CONSTRUCTORS
		/////////////////////////////////////////////////////////////////////

		public DeviceMarker(Device d)
			: base(d)
		{

		}

		public static Entity Create(Device device)
		{
			DeviceMarker marker = new DeviceMarker(device);
			return new Entity() { IsActive = false, IsVisible = false }
				.AddComponent(new Transform3D())
				.AddComponent(marker)
				//spike
				.AddChild
				(
					marker.spike = new Entity("spike")
					.AddComponent(new Transform3D()
					{
						Position = new Vector3(0.0f, 5.0f, 0.0f),
						Rotation = new Vector3(180.0f.ToRadians(), 0f, 0f)
					})
					.AddComponent(new MaterialsMap())
					.AddComponent(Model.CreateCone(8f, 6f, 8))
					.AddComponent(new ModelRenderer())
					.AddComponent(marker.AddCollider(new BoxCollider()))
				);
		}

		/////////////////////////////////////////////////////////////////////
		// PROTECTED METHODS
		/////////////////////////////////////////////////////////////////////

		protected override void Initialize()
		{
			base.Initialize();
			spike.FindComponent<MaterialsMap>().DefaultMaterial =
				spikeMat = new BasicMaterial("textures/white.png".Load(RenderManager.GraphicsDevice), typeof(NonPremultipliedAlpha))
				{
					LightingEnabled = true,
					AmbientLightColor = Color.White * 0.75f,
					DiffuseColor = Color.White,
					Alpha = 0.75f
				};
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

			spikeMat.Alpha = spikeMat.Alpha.Lerp(Entity.Selected ? 1.0f : 0.75f,
				(float)gameTime.TotalSeconds * FADE_SPEED);
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
