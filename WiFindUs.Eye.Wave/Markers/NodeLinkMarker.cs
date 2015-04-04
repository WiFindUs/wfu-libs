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
	public class NodeLinkMarker : LinkMarker
	{
		private NodeMarker fromNode, toNode;
		private NodeLink link;

		/////////////////////////////////////////////////////////////////////
		// PROPERTIES
		/////////////////////////////////////////////////////////////////////

		public NodeLink NodeLink
		{
			get { return link; }
			set
			{
				if (value == link)
					return;
				if (link != null)
				{
					link.OnNodeLinkActiveChanged -= NodeLinkChanged;
					link.OnNodeLinkSignalStrengthChanged -= NodeLinkChanged;
					link.OnNodeLinkSpeedChanged -= NodeLinkChanged;
				}

				link = value;

				if (link != null)
				{ 
					link.OnNodeLinkActiveChanged += NodeLinkChanged;
					link.OnNodeLinkSignalStrengthChanged += NodeLinkChanged;
					link.OnNodeLinkSpeedChanged += NodeLinkChanged;
				}
			}
		}

		/////////////////////////////////////////////////////////////////////
		// CONSTRUCTORS
		/////////////////////////////////////////////////////////////////////

		public NodeLinkMarker(ILinkableMarker fromNode, ILinkableMarker toNode)
			: base(fromNode, toNode) { }

		/////////////////////////////////////////////////////////////////////
		// PROTECTED METHODS
		/////////////////////////////////////////////////////////////////////

		protected override void Initialize()
		{
			base.Initialize();

			UpdateMarkerState();
		}

		protected override void Update(TimeSpan gameTime)
		{
			base.Update(gameTime);

			if (!Owner.IsVisible)
				return;
			Alpha = Alpha.Lerp(link.Start.Selected || link.End.Selected ? 0.7f : 0.0f,
				(float)gameTime.TotalSeconds * FADE_SPEED);
		}

		protected override void FromMarkerChanged(ILinkableMarker oldFromMarker)
		{
			if (oldFromMarker != null && oldFromMarker == fromNode)
			{
				fromNode.VisibleChanged -= NodeMarkerChanged;
				fromNode.Entity.OnNodeMeshPointChanged -= NodeChanged;
				fromNode.Entity.SelectedChanged -= EntitySelectedChanged;
			}

			fromNode = FromMarker as NodeMarker;

			if (fromNode != null)
			{
				fromNode.VisibleChanged += NodeMarkerChanged;
				fromNode.Entity.OnNodeMeshPointChanged += NodeChanged;
				fromNode.Entity.SelectedChanged += EntitySelectedChanged;
			}

			UpdateMarkerState();
		}

		protected override void ToMarkerChanged(ILinkableMarker oldToMarker)
		{
			if (oldToMarker != null && oldToMarker == toNode)
			{
				toNode.VisibleChanged -= NodeMarkerChanged;
				toNode.Entity.OnNodeMeshPointChanged -= NodeChanged;
				toNode.Entity.SelectedChanged -= EntitySelectedChanged;
			}

			toNode = ToMarker as NodeMarker;

			if (toNode != null)
			{
				toNode.VisibleChanged += NodeMarkerChanged;
				toNode.Entity.OnNodeMeshPointChanged += NodeChanged;
				toNode.Entity.SelectedChanged += EntitySelectedChanged;
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

		private void NodeLinkChanged(NodeLink nodeLink)
		{
			UpdateMarkerState();
		}

		private void NodeMarkerChanged(EntityMarker<Node> nodeMarker)
		{
			UpdateMarkerState();
		}

		private void EntitySelectedChanged(ISelectable entity)
		{
			UpdateMarkerState();
		}

		private void UpdateMarkerState()
		{
			if (Owner == null)
				return;
			bool newVisible = 
			(
				fromNode != null
				&& toNode != null
				&& toNode != fromNode
				&& fromNode.Entity != null
				&& toNode.Entity != null
				&& link != null
				&& link.Start != link.End
				&& (link.Start == fromNode.Entity || link.Start == toNode.Entity)
				&& (link.End == fromNode.Entity || link.End == toNode.Entity)
				&& link.Active
				&& fromNode.Owner.IsVisible
				&& toNode.Owner.IsVisible
				&& fromNode.Entity.MeshPoint.GetValueOrDefault()
				&& toNode.Entity.MeshPoint.GetValueOrDefault()
				&& (toNode.Selected || fromNode.Selected)
			);
			if (newVisible != Owner.IsVisible)
			{
				if (newVisible)
					Alpha = 0.0f;
				Owner.IsVisible = Owner.IsActive = newVisible;
			}

			if (Owner.IsVisible)
			{
				if (!link.SignalStrength.HasValue || link.SignalStrength > -30)
					Colour = Color.White;
				if (link.SignalStrength <= -30 && link.SignalStrength > -50)
					Colour = Color.Lime;
				else if (link.SignalStrength > -65)
					Colour = Color.LawnGreen;
				else if (link.SignalStrength > -70)
					Colour = Color.Yellow;
				else if (link.SignalStrength > -80)
					Colour = Color.Orange;
				else
					Colour = Color.Red;
			}
		}


	}
}
