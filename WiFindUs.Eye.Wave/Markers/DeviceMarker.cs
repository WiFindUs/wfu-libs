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
		private BasicMaterial spikeMat, coreMat;
		private Color colour = Color.White;
		private float fader = 0.0f;
		private StackPanel uiPanel;

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
					.AddComponent(new MaterialsMap(marker.spikeMat = new BasicMaterial(MapScene.WhiteTexture)
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
					.AddComponent(new MaterialsMap(marker.coreMat = new BasicMaterial(MapScene.WhiteTexture)
					{
						LayerType = typeof(NonPremultipliedAlpha),
						LightingEnabled = true,
						AmbientLightColor = Color.White * 0.75f,
						Alpha = 0.5f
					}))
					.AddComponent(Model.CreateSphere(3f, 4))
					.AddComponent(new ModelRenderer())
				)
				//selection
				.AddChild(SelectionRing.Create(device, 6.0f, 10, 7f).Owner);

			//ui
			marker.UIEntity = (marker.uiPanel = new StackPanel()
			{
				Orientation = Orientation.Vertical,
				BackgroundColor = Themes.Theme.Current.Background.Dark.Colour.Wave(200),
				IsBorder = false,
				Opacity = 0.0f,
				HorizontalAlignment = WaveEngine.Framework.UI.HorizontalAlignment.Left,
				VerticalAlignment = WaveEngine.Framework.UI.VerticalAlignment.Top
			}).Entity;
			marker.uiPanel.Add(new TextBlock() { Text = "adasdasdasd" });
			marker.UIEntity.FindComponent<Transform2D>().LocalScale = new Vector2(UI_SCALE);

			return marker;
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
			float targetAlpha = Entity.Active && (Entity.Selected || CameraTracking || CursorOver) ? 1.0f : 0.5f;
			spikeMat.Alpha = coreMat.Alpha = spikeMat.Alpha.Lerp(targetAlpha, secs * FADE_SPEED).Clamp(0.0f, 1.0f);
			spikeMat.DiffuseColor = Color.Lerp(spikeMat.DiffuseColor,
				!Entity.Active ? Color.DimGray : colour, secs * COLOUR_SPEED);
			if (Entity.Active)
				coreMat.DiffuseColor = Color.White.Coserp(Color.Cyan, fader += secs);

			//ui 
			uiPanel.Opacity = uiPanel.Opacity.Lerp(Entity.Selected || CursorOver ? 1.0f : 0.0f, secs * FADE_SPEED).Clamp(0.0f, 1.0f);
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
