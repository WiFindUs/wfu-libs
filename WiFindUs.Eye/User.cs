using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WiFindUs.Controls;

namespace WiFindUs.Eye
{
    public partial class User : ISelectableEntity
    {
        public static event Action<User> OnUserLoaded;
        public event Action<User> OnUserTypeChanged;
        public event Action<User> OnUserFirstNameChanged;
        public event Action<User> OnUserMiddleNameChanged;
        public event Action<User> OnUserNameLastChanged;
        public event Action<ISelectableEntity> SelectedChanged;
        private bool loaded = false, selected = false;

        /////////////////////////////////////////////////////////////////////
        // PROPERTIES
        /////////////////////////////////////////////////////////////////////

        public bool Selected
        {
            get { return selected; }
            set
            {
                if (selected == value)
                    return;
                selected = value;
                if (SelectedChanged != null)
                    SelectedChanged(this);
            }
        }

        /////////////////////////////////////////////////////////////////////
        // PRIVATE METHODS
        /////////////////////////////////////////////////////////////////////

        partial void OnLoaded()
        {
            loaded = true;
            Debugger.V(this.ToString() + " loaded.");
            if (OnUserLoaded != null)
                OnUserLoaded(this);
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

        public bool Loaded
        {
            get { return loaded; }
        }
    }
}
