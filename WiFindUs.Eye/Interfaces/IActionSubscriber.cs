using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WiFindUs.Eye
{
    public interface IActionSubscriber
    {
        bool ActionEnabled(uint index);
        Image ActionImage(uint index);
        String ActionText(uint index);
        void ActionTriggered(uint index);
        String ActionDescription { get; }
    }
}
