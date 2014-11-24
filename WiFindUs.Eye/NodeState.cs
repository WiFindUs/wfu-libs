using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WiFindUs.Eye
{
    public partial class NodeState : ILocation, INodeClient, ICreationTimestamped
    {
        public double DistanceTo(ILocation other)
        {
            return Location.Distance(this, other);
        }
    }
}
