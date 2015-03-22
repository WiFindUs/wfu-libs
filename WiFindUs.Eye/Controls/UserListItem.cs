using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using WiFindUs.Extensions;

namespace WiFindUs.Eye.Controls
{
    public class UserListItem : EntityListItem
    {
        private User user;
        
        /////////////////////////////////////////////////////////////////////
        // PROPERTIES
        /////////////////////////////////////////////////////////////////////

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public User User
        {
            get { return Entity as User; }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        protected override Color ImagePlaceholderColour
        {
            get
            {
                return WFUApplication.Config.Get("type_" + user.Type + ".colour", Color.Red);
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        protected override String EntityDetailString
        {
            get
            {
                if (user == null)
                    return "";
                return user.Type;
            }
        }

        /////////////////////////////////////////////////////////////////////
        // CONSTRUCTORS
        /////////////////////////////////////////////////////////////////////

        public UserListItem(User user)
            : base(user)
        {
            this.user = user;

            user.OnUserFirstNameChanged += user_OnUserFirstNameChanged;
            user.OnUserMiddleNameChanged += user_OnUserMiddleNameChanged;
            user.OnUserLastNameChanged += user_OnUserLastNameChanged;
            user.OnUserTypeChanged += user_OnUserTypeChanged;
        }

        /////////////////////////////////////////////////////////////////////
        // PROTECTED METHODS
        /////////////////////////////////////////////////////////////////////

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
        }

        /////////////////////////////////////////////////////////////////////
        // PRIVATE METHODS
        /////////////////////////////////////////////////////////////////////

        private void user_OnUserTypeChanged(User obj)
        {
            this.RefreshThreadSafe();
        }

        private void user_OnUserLastNameChanged(User obj)
        {
            this.RefreshThreadSafe();
        }

        private void user_OnUserMiddleNameChanged(User obj)
        {
            this.RefreshThreadSafe();
        }

        private void user_OnUserFirstNameChanged(User obj)
        {
            this.RefreshThreadSafe();
        }
    }
}
