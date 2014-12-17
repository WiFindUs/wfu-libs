using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WiFindUs.Eye
{
    public partial class NodeHistory : ILocation, ILocatable
    {
        public static event Action<NodeHistory> OnNodeHistoryLoaded;

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

        public ILocation Location
        {
            get { return this; }
        }
        
        public double DistanceTo(ILocation other)
        {
            return WiFindUs.Eye.Location.Distance(this, other);
        }

        /////////////////////////////////////////////////////////////////////
        // PRIVATE METHODS
        /////////////////////////////////////////////////////////////////////

        partial void OnLoaded()
        {
            Debugger.V(this.ToString() + " loaded.");
            if (OnNodeHistoryLoaded != null)
                OnNodeHistoryLoaded(this);
        }
    }
}
