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
			Node.OnNodeLoaded += NodeLoaded;
			UpdateMarkerState();
		}

		protected override void FromMarkerChanged(ILinkableMarker oldFromMarker)
		{
			if (oldFromMarker != null && oldFromMarker == fromDevice)
			{
				fromDevice.VisibleChanged -= DeviceMarkerChanged;
				fromDevice.Entity.OnDeviceIPAddressChanged -= DeviceIPAddressChanged;
			}

			fromDevice = FromMarker as DeviceMarker;

			if (fromDevice != null)
			{
				fromDevice.VisibleChanged += DeviceMarkerChanged;
				fromDevice.Entity.OnDeviceIPAddressChanged += DeviceIPAddressChanged;
			}

			UpdateMarkerState();
		}

		protected override void ToMarkerChanged(ILinkableMarker oldToMarker)
		{
			if (oldToMarker != null && oldToMarker == toNode)
			{
				toNode.VisibleChanged -= NodeMarkerChanged;
				toNode.Entity.OnAPDaemonRunningChanged -= NodeChanged;
			}

			toNode = ToMarker as NodeMarker;

			if (toNode != null)
			{
				toNode.VisibleChanged += NodeMarkerChanged;
				toNode.Entity.OnAPDaemonRunningChanged += NodeChanged;
			}

			UpdateMarkerState();
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

		private void NodeLoaded(Node node)
		{
			CheckForConnectionWithNewNode();
			node.OnNodeNumberChanged += NodeNumberChanged;
		}

		private void NodeNumberChanged(Node obj)
		{
			CheckForConnectionWithNewNode();
		}

		private void NodeChanged(Node node)
		{
			UpdateMarkerState();
		}

		private void DeviceIPAddressChanged(Device device)
		{
			ToMarker = this.Scene.GetNodeMarker(fromDevice.Entity.ConnectedNodeNumber);
		}

		private void DeviceMarkerChanged(EntityMarker<Device> deviceMarker)
		{
			UpdateMarkerState();
		}

		private void NodeMarkerChanged(EntityMarker<Node> nodeMarker)
		{
			UpdateMarkerState();
		}

		private void UpdateMarkerState()
		{
			if (Owner == null)
				return;
			Owner.IsActive = Owner.IsVisible =
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
