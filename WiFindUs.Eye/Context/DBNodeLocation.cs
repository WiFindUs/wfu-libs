using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WiFindUs.Eye.Context
{
    public partial class DBNodeLocation : ILocation, INodeClient, ICreationTimestamped
    {
        public double DistanceTo(ILocation other)
        {
            return Location.Distance(this, other);
        }

        public INode Node
        {
            get { return NodeDB; }
        }
    }
}
