using System;
namespace WiFindUs.Eye
{
    public interface IUser
    {
        DateTime Created { get; }
        string NameFirst { get; set; }
        string NameLast { get; set; }
        string NameMiddle { get; set; }
        string Type { get; set; }
        long ID { get; }
    }
}
