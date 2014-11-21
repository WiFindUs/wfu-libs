using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WiFindUs.Eye.Context
{
    public partial class DBNode : INode, IIndentifiable, ILocatable, ICreationTimestamped
    {
        public List<ILocation> Locations
        {
            get
            {
                return new List<ILocation>(LocationsDB);
            }
        }

        public ILocation CurrentLocation
        {
            get
            {
                ICreationTimestamped current = null;
                foreach (DBNodeLocation location in LocationsDB)
                    if (current == null || location.Created.Ticks > current.Created.Ticks)
                        current = location;
                return current as ILocation;
            }
        }
    }
}
