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
		private double timer = -1.0;

		/////////////////////////////////////////////////////////////////////
		// CONSTRUCTORS
		/////////////////////////////////////////////////////////////////////

		public DeviceLinkMarker(ILinkableMarker fromDevice, ILinkableMarker toNode)
			: base(fromDevice, toNode) { }

		/////////////////////////////////////////////////////////////////////
		// PROTECTED METHODS
		/////////////////////////////////////////////////////////////////////

		protected override void Initialize()
		{
			base.Initialize();
			UpdateMarkerState();
		}

		protected override void FromMarkerChanged(ILinkableMarker oldFromMarker)
		{
			if (oldFromMarker != null && oldFromMarker == fromDevice)
			{
				fromDevice.VisibleChanged -= DeviceVisibleChanged;
				fromDevice.Entity.OnDeviceIPAddressChanged -= DeviceIPAddressChanged;
			}

			fromDevice = FromMarker as DeviceMarker;

			if (fromDevice != null)
			{
				fromDevice.VisibleChanged += DeviceVisibleChanged;
				fromDevice.Entity.OnDeviceIPAddressChanged += DeviceIPAddressChanged;
			}

			UpdateMarkerState();
		}

		protected override void ToMarkerChanged(ILinkableMarker oldToMarker)
		{
			if (oldToMarker != null && oldToMarker == toNode)
			{
				toNode.VisibleChanged -= NodeVisibleChanged;
				toNode.Entity.OnAPDaemonRunningChanged -= NodeChanged;
				toNode.Entity.OnNodeNumberChanged -= NodeNumberChanged;
			}

			toNode = ToMarker as NodeMarker;

			if (toNode != null)
			{
				toNode.VisibleChanged += NodeVisibleChanged;
				toNode.Entity.OnAPDaemonRunningChanged += NodeChanged;
				toNode.Entity.OnNodeNumberChanged += NodeNumberChanged;
			}

			UpdateMarkerState();
		}

		protected override void Update(TimeSpan gameTime)
		{
			base.Update(gameTime);

			timer -= gameTime.TotalSeconds;
			if (timer < 0.0)
			{
				CheckForConnectionWithNewNode();
				timer = 5.0;
			}
		}

		/////////////////////////////////////////////////////////////////////
		// PRIVATE METHODS
		/////////////////////////////////////////////////////////////////////

		private void CheckForConnectionWithNewNode()
		{
			uint nodeNum = fromDevice.Entity.ConnectedNodeNumber;
			if (nodeNum == 0 || (toNode != null && toNode.Entity.Number.GetValueOrDefault() == nodeNum))
				return;
			ToMarker = this.Scene.GetNodeMarker(fromDevice.Entity.ConnectedNodeNumber);
		}

		private void NodeNumberChanged(Node node)
		{
			ToMarker = this.Scene.GetNodeMarker(fromDevice.Entity.ConnectedNodeNumber);
		}

		private void NodeChanged(Node node)
		{
			UpdateMarkerState();
		}

		private void DeviceIPAddressChanged(Device device)
		{
			ToMarker = this.Scene.GetNodeMarker(fromDevice.Entity.ConnectedNodeNumber);
		}

		private void DeviceVisibleChanged(EntityMarker<Device> deviceMarker)
		{
			UpdateMarkerState();
		}

		private void NodeVisibleChanged(EntityMarker<Node> nodeMarker)
		{
			UpdateMarkerState();
		}

		private void UpdateMarkerState()
		{
			if (Owner == null)
				return;
			Owner.IsVisible =
			(
				fromDevice != null
				&& toNode != null
				&& fromDevice.Owner.IsVisible
				&& toNode.Owner.IsVisible
				&& fromDevice.Entity.ConnectedNodeNumber > 0
				&& toNode.Entity.IsAPDaemonRunning.GetValueOrDefault()
			);
		}
	}
}
