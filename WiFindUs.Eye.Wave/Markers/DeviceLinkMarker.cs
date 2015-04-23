using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WaveEngine.Common.Graphics;
using WaveEngine.Common.Math;
using WaveEngine.Components.Graphics3D;
using WaveEngine.Framework;
using WaveEngine.Framework.Graphics;
using WaveEngine.Materials;
using WiFindUs.Extensions;
using WiFindUs.Eye.Wave.Layers;

namespace WiFindUs.Eye.Wave.Markers
{
	public class DeviceLinkMarker : LinkMarker
	{
		private DeviceMarker fromDevice;
		private NodeMarker toNode;
		private double timer = 1.0;

		/////////////////////////////////////////////////////////////////////
		// CONSTRUCTORS
		/////////////////////////////////////////////////////////////////////

		internal DeviceLinkMarker(ILinkableMarker fromDevice, ILinkableMarker toNode)
			: base(fromDevice, toNode) { }

		public DeviceLinkMarker() : base() { }

		/////////////////////////////////////////////////////////////////////
		// PROTECTED METHODS
		/////////////////////////////////////////////////////////////////////

		protected override void Initialize()
		{
			base.Initialize();
			Diameter = 2.5f;
			IsOwnerActive = true;
			UpdateNodeLink();
		}

		protected override void FromMarkerChanged(ILinkableMarker oldFromMarker)
		{
			if (oldFromMarker != null && oldFromMarker == fromDevice)
			{
				fromDevice.VisibleChanged -= DeviceVisibleChanged;
				fromDevice.Entity.OnDeviceNodeChanged-= DeviceNodeChanged;
				fromDevice.Entity.SelectedChanged -= EntitySelectedChanged;
				
			}

			fromDevice = FromMarker as DeviceMarker;

			if (fromDevice != null)
			{
				fromDevice.VisibleChanged += DeviceVisibleChanged;
				fromDevice.Entity.OnDeviceNodeChanged += DeviceNodeChanged;
				fromDevice.Entity.SelectedChanged += EntitySelectedChanged;
			}

			UpdateMarkerState();
		}

		protected override void ToMarkerChanged(ILinkableMarker oldToMarker)
		{
			if (oldToMarker != null && oldToMarker == toNode)
			{
				toNode.VisibleChanged -= NodeVisibleChanged;
				toNode.Entity.OnNodeAccessPointChanged -= NodeChanged;
				toNode.Entity.OnNodeNumberChanged -= NodeChanged;
				toNode.Entity.SelectedChanged -= EntitySelectedChanged;
			}

			toNode = ToMarker as NodeMarker;

			if (toNode != null)
			{
				toNode.VisibleChanged += NodeVisibleChanged;
				toNode.Entity.OnNodeAccessPointChanged += NodeChanged;
				toNode.Entity.OnNodeNumberChanged += NodeChanged;
				toNode.Entity.SelectedChanged += EntitySelectedChanged;
			}

			UpdateMarkerState();
		}

		protected override void Update(TimeSpan gameTime)
		{
			base.Update(gameTime);
			timer -= gameTime.TotalSeconds;
			if (timer < 0.0)
			{
				UpdateNodeLink();
				timer = 5.0;
			}

			float secs = (float)gameTime.TotalSeconds;

			if (!Owner.IsVisible)
				return;
			Alpha = Alpha.Lerp(fromDevice.Selected || toNode.Selected ? 0.35f : 0.0f, secs * FADE_SPEED).Clamp(0.0f, 1.0f);
			Colour = Color.Lerp(Colour,
				!fromDevice.Entity.Active || !toNode.Entity.Active ? inactiveLinkColour : Color.Yellow,
				secs * COLOUR_SPEED);
		}

		/////////////////////////////////////////////////////////////////////
		// PRIVATE METHODS
		/////////////////////////////////////////////////////////////////////

		private void UpdateNodeLink()
		{
			if (fromDevice.Entity.Node == null)
				ToMarker = null;

			if (toNode == null || toNode.Entity == null || toNode.Entity != fromDevice.Entity.Node)
				ToMarker = MapScene.Markers.OfType<NodeMarker>().Where(mk => mk.Entity == fromDevice.Entity.Node).FirstOrDefault();
			

			UpdateMarkerState();
		}

		private void EntitySelectedChanged(ISelectable entity)
		{
			UpdateMarkerState();
		}

		private void NodeChanged(Node node)
		{
			UpdateMarkerState();
		}

		private void NodeVisibleChanged(EntityMarker<Node> nodeMarker)
		{
			UpdateMarkerState();
		}

		private void DeviceVisibleChanged(EntityMarker<Device> deviceMarker)
		{
			UpdateMarkerState();
		}

		private void DeviceNodeChanged(Device device)
		{
			UpdateNodeLink();
		}

		private void UpdateMarkerState()
		{
			if (Owner == null)
				return;
			bool newVisible = 
			(
				fromDevice != null
				&& toNode != null
				&& fromDevice.Owner != null
				&& toNode.Owner != null
				&& fromDevice.IsOwnerVisible
				&& toNode.IsOwnerVisible
				&& toNode.Entity.AccessPoint.GetValueOrDefault()
				&& (toNode.Entity.Selected || fromDevice.Entity.Selected)
			);

			if (newVisible != IsOwnerVisible)
			{
				if (newVisible)
					Alpha = 0.0f;
				IsOwnerVisible = newVisible;
			}
		}
	}
}
