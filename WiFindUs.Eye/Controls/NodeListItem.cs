using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WiFindUs.Controls;
using WiFindUs.Extensions;

namespace WiFindUs.Eye.Controls
{
    public class NodeListItem : EntityListItem
    {
        private Node node;
        
        /////////////////////////////////////////////////////////////////////
        // PROPERTIES
        /////////////////////////////////////////////////////////////////////

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Node Node
        {
            get { return node; }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        protected override String EntityTitleString
        {
            get { return String.Format("Node #{0:X}", node.ID); }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        protected override String EntityDetailString
        {
            get
            {
                return String.Format("{0}\n{1}",
                    node.TimedOut ? "Timed out." : "Assigned to station #" + node.Number,
                    node.TimedOut ? "" : (node.HasLatLong ? WiFindUs.Eye.Location.ToString(node) : ""));
            }
        }

        /////////////////////////////////////////////////////////////////////
        // CONSTRUCTORS
        /////////////////////////////////////////////////////////////////////

        public NodeListItem(Node node)
            : base(node)
        {
            this.node = node;

            node.LocationChanged += node_LocationChanged;
            node.OnAPDaemonRunningChanged += node_RunningDaemonsChanged;
            node.OnDHCPDaemonRunningChanged += node_RunningDaemonsChanged;
            node.OnGPSDaemonRunningChanged += node_RunningDaemonsChanged;
            node.OnMeshPointChanged += node_RunningDaemonsChanged;
            node.OnMeshPeersChanged +=node_OnMeshPeersChanged;
            node.OnNodeIPAddressChanged += node_OnNodeIPAddressChanged;
            node.OnNodeNumberChanged += node_OnNodeNumberChanged;
            node.OnNodeVoltageChanged += node_OnNodeVoltageChanged;
            node.OnVisibleSatellitesChanged += node_OnVisibleSatellitesChanged;
            node.TimedOutChanged += node_TimedOutChanged;
            node.WhenUpdated += node_WhenUpdated;
        }

        private void node_WhenUpdated(IUpdateable obj)
        {
            this.RefreshThreadSafe();
        }

        private void node_TimedOutChanged(IUpdateable obj)
        {
            this.RefreshThreadSafe();
        }

        private void node_OnVisibleSatellitesChanged(Node obj)
        {
            this.RefreshThreadSafe();
        }

        private void node_OnNodeVoltageChanged(Node obj)
        {
            this.RefreshThreadSafe();
        }

        private void node_OnNodeNumberChanged(Node obj)
        {
            this.RefreshThreadSafe();
        }

        private void node_OnNodeIPAddressChanged(Node obj)
        {
            this.RefreshThreadSafe();
        }

        private void node_OnMeshPeersChanged(Node obj)
        {
            this.RefreshThreadSafe();
        }

        private void node_RunningDaemonsChanged(Node obj)
        {
            this.RefreshThreadSafe();
        }

        private void node_LocationChanged(ILocatable obj)
        {
            this.RefreshThreadSafe();
        }

        /////////////////////////////////////////////////////////////////////
        // PROTECTED METHODS
        /////////////////////////////////////////////////////////////////////

        protected override int CalculateHeight()
        {
            return (base.CalculateHeight() * 3) / 2;
        }
    }
}
