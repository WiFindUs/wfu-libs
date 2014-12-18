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
    public class WaypointListItem : EntityListItem
    {
        private Waypoint incident;

        /////////////////////////////////////////////////////////////////////
        // PROPERTIES
        /////////////////////////////////////////////////////////////////////

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Waypoint Waypoint
        {
            get { return Entity as Waypoint; }
        }

        /////////////////////////////////////////////////////////////////////
        // CONSTRUCTORS
        /////////////////////////////////////////////////////////////////////

        public WaypointListItem(Waypoint waypoint)
            : base(waypoint)
        {

        }
    }
}
