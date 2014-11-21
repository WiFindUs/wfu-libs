using Devart.Data.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
namespace WiFindUs.Eye
{
    public interface IWaypoint
    {
        bool Archived { get; set; }
        List<IUser> ArchivedResponders { get; }
        DateTime? ArchivedTime { get; }
        List<IDevice> AssignedDevices { get; }
        string Category { get; set; }
        int Code { get; set; }
        DateTime Created { get; }
        string Description { get; set; }
        IWaypoint NextWaypoint { get; set; }
        IUser ReportingUser { get; set; }
        int Severity { get; set; }
        string Type { get; set; }
        long ID { get; }
    }
}
