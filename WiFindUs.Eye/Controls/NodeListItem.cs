using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using WiFindUs.Extensions;
using WiFindUs.Themes;

namespace WiFindUs.Eye.Controls
{
    public class NodeListItem : EntityListItem
	{
		private Node node;

		/////////////////////////////////////////////////////////////////////
		// PROPERTIES
		/////////////////////////////////////////////////////////////////////

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public Node Node
		{
			get { return node; }
		}

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		protected override String EntityTitleString
		{
			get
			{
				if (node == null)
					return "";
				return String.Format("Node #{0:X}", node.ID);
			}
		}

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		protected override String EntityDetailString
		{
			get
			{
				if (node == null)
					return "";
				return String.Format("{0}\n{1}\n\n",
					node.Number.HasValue ? "Assigned to station #" + node.Number.Value : "Not assigned to station.",
					!node.Active ? "Inactive." :
						(node.GPSD.GetValueOrDefault() ? (node.HasLatLong ? WiFindUs.Location.ToString(node) : "Waiting for accurate location...")
							: "GPS not running."));
			}
		}

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		protected override Color ImagePlaceholderColour
		{
			get { return node.AccessPointColor; }
		}

		/////////////////////////////////////////////////////////////////////
		// CONSTRUCTORS
		/////////////////////////////////////////////////////////////////////

		public NodeListItem(Node node)
			: base(node)
		{
			this.node = node;
			if (IsDesignMode)
				return;
			node.LocationChanged += node_LocationChanged;
			node.OnNodeAccessPointChanged += node_Changed;
			node.OnNodeDHCPDChanged += node_Changed;
			node.OnNodeGPSDChanged += node_Changed;
			node.OnNodeMeshPointChanged += node_Changed;
			node.OnNodeNumberChanged += node_Changed;
			node.OnNodeVoltageChanged += node_Changed;
			node.OnNodeSatelliteCountChanged += node_Changed;
			node.Updated += node_Updated;
			node.ActiveChanged += node_Updated;
			node.OnNodeMockLocationChanged += node_Changed;
		}

		/////////////////////////////////////////////////////////////////////
		// PROTECTED METHODS
		/////////////////////////////////////////////////////////////////////
		protected override void OnPaint(PaintEventArgs e)
		{
			base.OnPaint(e);

			if (node == null || !node.Active)
				return;

			NodeListStatusItem[] statuses = new NodeListStatusItem[] {
				new NodeListStatusItem("MP", node.MeshPoint),
				new NodeListStatusItem("AP", node.AccessPoint),
				new NodeListStatusItem("DHCP", node.DHCPD),
				new NodeListStatusItem("GPS", node.GPSD)
			};

			using (Font f = new Font(Font.FontFamily, Font.Size - 2.0f))
			{
				int hstep = ClientRectangle.Width / 5;
				int vstep = f.Height * 2;
				for (int i = 0; i < statuses.Length; i++)
					statuses[i].Paint(e.Graphics, f, ((i % 4) + 1) * hstep, 48 + (i / 4) * vstep);
			}
		}

		/////////////////////////////////////////////////////////////////////
		// PRIVATE METHODS
		/////////////////////////////////////////////////////////////////////

		private class NodeListStatusItem
		{
			public readonly string Caption;
			public readonly bool? Status;
			private readonly string okText, failText, waitingText;

			public NodeListStatusItem(string caption, bool? status, string okText = "OK", string failText = "Fail", string waitingText = "Waiting")
			{
				Caption = caption ?? "";
				Status = status;
				this.okText = okText ?? "OK";
				this.failText = failText ?? "Fail";
				this.waitingText = waitingText ?? "Waiting";
			}

			public void Paint(Graphics g, Font f, int x, int y)
			{
				using (StringFormat sf = new StringFormat(StringFormat.GenericTypographic) { Alignment = StringAlignment.Center })
				{
					g.DrawString(Caption + ":\n" +
					(Status.HasValue ? (Status.Value ? okText : failText) : waitingText),
					f,
					(Status.HasValue ? (Status.Value ? Theme.Current.OK.Mid.Brush : Theme.Current.Error.Mid.Brush) : Theme.Current.Warning.Mid.Brush),
					x, y, sf);
				}
			}
		}

		private void node_Updated(IUpdateable obj)
		{
			this.InvalidateThreadSafe();
		}

		private void node_Changed(Node obj)
		{
			this.InvalidateThreadSafe();
		}

		private void node_LocationChanged(ILocatable obj)
		{
			this.InvalidateThreadSafe();
		}
	}
}
