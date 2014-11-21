using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace WiFindUs.Eye
{
    public interface ILocatable
    {
        List<ILocation> Locations { get; }
        ILocation CurrentLocation { get; }
    }
}
