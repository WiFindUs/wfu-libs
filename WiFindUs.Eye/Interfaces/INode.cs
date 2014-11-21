using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
namespace WiFindUs.Eye
{
    public interface INode
    {
        DateTime Created { get; }
        long ID { get; }
        List<ILocation> Locations { get; }
        ILocation CurrentLocation { get; }
        long Number { get; }
        DateTime Updated { get; }
    }
}
