using System;
using System.ComponentModel;
using System.Drawing;
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
            get
            {
                if (node == null)
                    return "";
                return String.Format("Node #{0:X}", node.ID);
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        protected override String EntityDetailString
        {
            get
            {
                if (node == null)
                    return "";
                return String.Format("{0}\n{1}\n\n",
                    node.Number.HasValue ? "Assigned to station #" + node.Number.Value : "Not assigned to station." ,
                    node.TimedOut ? "Timed out." :
                        (node.IsGPSDaemonRunning.GetValueOrDefault() ? (node.HasLatLong ? WiFindUs.Eye.Location.ToString(node) : "Waiting for accurate location...")
                            : "GPS not running."));
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

        /////////////////////////////////////////////////////////////////////
        // PROTECTED METHODS
        /////////////////////////////////////////////////////////////////////
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            if (node == null || node.TimedOut)
                return;

            NodeListStatusItem[] statuses = new NodeListStatusItem[] {
                new NodeListStatusItem("MP", node.IsMeshPoint),
                new NodeListStatusItem("AP", node.IsAPDaemonRunning),
                new NodeListStatusItem("DHCP", node.IsDHCPDaemonRunning),
                new NodeListStatusItem("GPS", node.IsGPSDaemonRunning)
            };

            using (Font f = new Font(Font.FontFamily, Font.Size - 2.0f))
            {
                int hstep = ClientRectangle.Width / 5;
                int vstep = f.Height * 2;
                for (int i = 0; i < statuses.Length; i++)
                    statuses[i].Paint(e.Graphics, Theme, f, ((i % 4) + 1) * hstep, 48 + (i / 4) * vstep);
            }
        }

        /////////////////////////////////////////////////////////////////////
        // PRIVATE METHODS
        /////////////////////////////////////////////////////////////////////

        private class NodeListStatusItem
        {
            public readonly String Caption;
            public readonly bool? Status;

            public NodeListStatusItem(string caption, bool? status)
            {
                Caption = caption;
                Status = status;
            }

            public void Paint(Graphics g, Theme t, Font f, int x, int y)
            {
                using (StringFormat sf = new StringFormat(StringFormat.GenericTypographic) { Alignment = StringAlignment.Center })
                {
                    g.DrawString(Caption + ":\n" +
                    (Status.HasValue ? (Status.Value ? "OK" : "Fail") : "Waiting"),
                    f,
                    (Status.HasValue ? (Status.Value ? t.OKBrush : t.ErrorBrush) : t.WarningBrush),
                    x, y, sf);
                }
            }
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
    }
}
