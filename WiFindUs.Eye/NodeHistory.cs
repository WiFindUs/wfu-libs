using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WiFindUs.Eye
{
    public partial class NodeHistory : ILocation
    {
        public static event Action<NodeHistory> OnNodeHistoryLoaded;
        private bool loaded = false;

        /////////////////////////////////////////////////////////////////////
        // PROPERTIES
        /////////////////////////////////////////////////////////////////////
        
        public bool HasLatLong
        {
            get
            {
                return Latitude.HasValue
                    && Longitude.HasValue;
            }
        }

        public bool EmptyLocation
        {
            get
            {
                return !Latitude.HasValue
                    && !Longitude.HasValue
                    && !Accuracy.HasValue
                    && !Altitude.HasValue;
            }
        }
       
        public double DistanceTo(ILocation other)
        {
            return WiFindUs.Eye.Location.Distance(this, other);
        }

        public bool Loaded
        {
            get { return loaded; }
        }

        /////////////////////////////////////////////////////////////////////
        // PRIVATE METHODS
        /////////////////////////////////////////////////////////////////////

        partial void OnLoaded()
        {
            loaded = true;
            Debugger.V(this.ToString() + " loaded.");
            if (OnNodeHistoryLoaded != null)
                OnNodeHistoryLoaded(this);
        }
    }
}
