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
                return new List<ILocation>(DBNodeStates);
            }
        }

        public DBNodeState CurrentState
        {
            get
            {
                DBNodeState current = null;
                foreach (DBNodeState state in DBNodeStates)
                    if (current == null || state.Created.Ticks > current.Created.Ticks)
                        current = state;
                return current;
            }
        }

        public ILocation CurrentLocation
        {
            get
            {
                return CurrentState as ILocation;
            }
        }

        public DateTime Updated
        {
            get { return CurrentState.Created; }
        }

        public long Number
        {
            get { return CurrentState.Number; }
        }
    }
}
