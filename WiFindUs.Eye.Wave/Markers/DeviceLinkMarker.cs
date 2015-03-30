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
			UpdateMarkerState();
		}

		protected override void FromMarkerChanged(ILinkableMarker oldFromMarker)
		{
			if (oldFromMarker != null && oldFromMarker == fromDevice)
			{
				fromDevice.VisibleChanged -= DeviceVisibleChanged;
				fromDevice.Entity.OnDeviceNodeChanged-= DeviceNodeChanged;
			}

			fromDevice = FromMarker as DeviceMarker;

			if (fromDevice != null)
			{
				fromDevice.VisibleChanged += DeviceVisibleChanged;
				fromDevice.Entity.OnDeviceNodeChanged += DeviceNodeChanged;
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

		/////////////////////////////////////////////////////////////////////
		// PRIVATE METHODS
		/////////////////////////////////////////////////////////////////////

		private void NodeNumberChanged(Node node)
		{
			ToMarker = this.Scene.GetNodeMarker(fromDevice.Entity.Node);
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
			ToMarker = this.Scene.GetNodeMarker(fromDevice.Entity.Node);
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
				&& toNode.Entity.IsAPDaemonRunning.GetValueOrDefault()
			);
		}
	}
}
