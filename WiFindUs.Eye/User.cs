using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WiFindUs.Eye
{
    public class User : IUser, IIndentifiable, ICreationTimestamped
    {
        public DateTime Created
        {
            get { throw new NotImplementedException(); }
        }

        public string NameFirst
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public string NameLast
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public string NameMiddle
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public string Type
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public long ID
        {
            get { throw new NotImplementedException(); }
        }
    }
}
