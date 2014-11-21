using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace WiFindUs.Eye
{
    public interface IDevice
    {
        IWaypoint AssignedWaypoint { get; set; }
        List<IAtmosphere> Atmospheres { get; }
        IAtmosphere CurrentAtmosphere { get; }
        DateTime Created { get; }
        List<ILocation> Locations { get; }
        ILocation CurrentLocation { get; }
        List<IDeviceLogin> Logins { get; }
        IUser CurrentUser { get; }
        string Type { get; }
        DateTime Updated { get; }
        long ID { get; }
    }
}
