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
	public class MeshLinkMarker : LinkMarker
	{
		private NodeMarker fromNode, toNode;

		/////////////////////////////////////////////////////////////////////
		// CONSTRUCTORS
		/////////////////////////////////////////////////////////////////////

		public MeshLinkMarker(ILinkableMarker fromNode, ILinkableMarker toNode)
			: base(fromNode, toNode) { }

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
			if (oldFromMarker != null && oldFromMarker == fromNode)
			{
				fromNode.VisibleChanged -= NodeMarkerChanged;
				fromNode.Entity.OnMeshPointChanged -= NodeChanged;
				fromNode.Entity.OnMeshPeersChanged -= NodeChanged;
			}

			fromNode = FromMarker as NodeMarker;

			if (fromNode != null)
			{
				fromNode.VisibleChanged += NodeMarkerChanged;
				fromNode.Entity.OnMeshPointChanged += NodeChanged;
				fromNode.Entity.OnMeshPeersChanged += NodeChanged;
			}

			UpdateMarkerState();
		}

		protected override void ToMarkerChanged(ILinkableMarker oldToMarker)
		{
			if (oldToMarker != null && oldToMarker == toNode)
			{
				toNode.VisibleChanged -= NodeMarkerChanged;
				toNode.Entity.OnMeshPointChanged -= NodeChanged;
				toNode.Entity.OnMeshPeersChanged -= NodeChanged;
			}

			toNode = ToMarker as NodeMarker;

			if (toNode != null)
			{
				toNode.VisibleChanged += NodeMarkerChanged;
				toNode.Entity.OnMeshPointChanged += NodeChanged;
				toNode.Entity.OnMeshPeersChanged += NodeChanged;
			}

			UpdateMarkerState();
		}

		/////////////////////////////////////////////////////////////////////
		// PRIVATE METHODS
		/////////////////////////////////////////////////////////////////////

		private void NodeChanged(Node node)
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
				fromNode != null
				&& toNode != null
				&& toNode != fromNode
				&& fromNode.Owner.IsVisible
				&& toNode.Owner.IsVisible
				&& fromNode.Entity.IsMeshPoint.GetValueOrDefault()
				&& toNode.Entity.IsMeshPoint.GetValueOrDefault()
				&& fromNode.Entity.MeshPeerCount > 0
				&& toNode.Entity.MeshPeerCount > 0
				&& fromNode.Entity.MeshPeers.Contains(toNode.Entity)
				&& toNode.Entity.MeshPeers.Contains(fromNode.Entity)
			);
		}
	}
}
