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
    public class IncidentListChild : EntityListChild
    {
        private Waypoint incident;

        /////////////////////////////////////////////////////////////////////
        // PROPERTIES
        /////////////////////////////////////////////////////////////////////

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Waypoint Incident
        {
            get { return incident; }
        }

        /////////////////////////////////////////////////////////////////////
        // CONSTRUCTORS
        /////////////////////////////////////////////////////////////////////

        public IncidentListChild(Waypoint incident)
        {
            if (incident == null)
                throw new ArgumentNullException("incident", "Incident waypoint cannot be null!");
            this.incident = incident;
        }
    }
}
