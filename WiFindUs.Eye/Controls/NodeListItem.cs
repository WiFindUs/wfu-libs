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
