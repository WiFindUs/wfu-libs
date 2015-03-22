using WaveEngine.Common.Graphics;
using WaveEngine.Common.Math;
using WaveEngine.Components.Graphics3D;
using WaveEngine.Framework;
using WaveEngine.Framework.Graphics;
using WaveEngine.Framework.Physics3D;
using WiFindUs.Extensions;

namespace WiFindUs.Eye.Wave.Markers
{
	public class DeviceMarker : EntityMarker<Device>
	{
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
					new Entity("spike")
					.AddComponent(new Transform3D()
					{
						Position = new Vector3(0.0f, 5.0f, 0.0f),
						Rotation = new Vector3(180.0f.ToRadians(), 0f, 0f)
					})
					.AddComponent(new MaterialsMap(PlaceHolderMaterial))
					.AddComponent(Model.CreateCone(10f, 6f, 5))
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
			entity.OnDeviceUserChanged += OnDeviceUserChanged;
			entity.OnGPSEnabledChanged += OnDeviceGPSStateChanged;
			entity.OnGPSFixedChanged += OnDeviceGPSStateChanged;
			UpdateMarkerState();
		}

		protected override bool UpdateVisibilityCheck()
		{
			return entity.IsGPSEnabled.GetValueOrDefault()
				&& entity.IsGPSFixed.GetValueOrDefault() && base.UpdateVisibilityCheck();
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
