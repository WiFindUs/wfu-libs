using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WiFindUs.Controls;
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

        /////////////////////////////////////////////////////////////////////
        // CONSTRUCTORS
        /////////////////////////////////////////////////////////////////////

        public UserListItem(User user)
            : base(user)
        {

        }
    }
}
