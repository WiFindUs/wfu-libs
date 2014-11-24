using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WiFindUs.Eye
{
    public partial class User : IIndentifiable, ICreationTimestamped
    {
        public delegate void UserEvent(User sender);
        public static event UserEvent OnUserCreated;
        public event UserEvent OnUserTypeChanged;
        public event UserEvent OnUserFirstNameChanged;
        public event UserEvent OnUserMiddleNameChanged;
        public event UserEvent OnUserNameLastChanged;

        /////////////////////////////////////////////////////////////////////
        // PRIVATE METHODS
        /////////////////////////////////////////////////////////////////////

        partial void OnCreated()
        {
            if (OnUserCreated != null)
                OnUserCreated(this);
        }

        partial void OnNameFirstChanged()
        {
            if (OnUserFirstNameChanged != null)
                OnUserFirstNameChanged(this);
        }

        partial void OnNameMiddleChanged()
        {
            if (OnUserMiddleNameChanged != null)
                OnUserMiddleNameChanged(this);
        }

        partial void OnNameLastChanged()
        {
            if (OnUserNameLastChanged != null)
                OnUserNameLastChanged(this);
        }

        partial void OnTypeChanged()
        {
            if (OnUserTypeChanged != null)
                OnUserTypeChanged(this);
        }
    }
}
