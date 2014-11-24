using System;
namespace WiFindUs.Eye
{
    public interface IDeviceLogin
    {
        DateTime Created { get; }
        Device Device { get; }
        User User { get; }
    }
}
