using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WiFindUs.Eye
{
    public class Node : INode, IIndentifiable, ILocatable, ICreationTimestamped
    {
        public DateTime Created
        {
            get { throw new NotImplementedException(); }
        }

        public long ID
        {
            get { throw new NotImplementedException(); }
        }

        public List<ILocation> Locations
        {
            get { throw new NotImplementedException(); }
        }

        public ILocation CurrentLocation
        {
            get { throw new NotImplementedException(); }
        }

        public long Number
        {
            get { throw new NotImplementedException(); }
        }

        public DateTime Updated
        {
            get { throw new NotImplementedException(); }
        }
    }
}
