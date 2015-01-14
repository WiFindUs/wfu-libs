using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace WiFindUs.Eye
{
    public interface ILocatable
    {
        ILocation Location { get; }
        event Action<ILocatable> LocationChanged;
    }
}
