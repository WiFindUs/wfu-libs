using System;
using System.Drawing;

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
