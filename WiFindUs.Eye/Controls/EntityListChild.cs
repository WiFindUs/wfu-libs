using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WiFindUs.Controls;
using WiFindUs.Extensions;

namespace WiFindUs.Eye.Controls
{
    public class EntityListChild : ThemedPanel
    {
        public override Color ThemeBackColor
        {
            get { return Theme.ControlDarkColour; }
        }
        
        /////////////////////////////////////////////////////////////////////
        // CONSTRUCTORS
        /////////////////////////////////////////////////////////////////////

        public EntityListChild()
        {
            Height = 40;
        }
    }
}
