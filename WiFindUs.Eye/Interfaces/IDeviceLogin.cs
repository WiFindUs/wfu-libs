using System;
namespace WiFindUs.Eye
{
    public interface IDeviceLogin
    {
        DateTime Created { get; }
        IDevice Device { get; }
        IUser User { get; }
    }
}
