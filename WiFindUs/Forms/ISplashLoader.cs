using System;
using System.Collections.Generic;

namespace WiFindUs.Forms
{
    public interface ISplashLoader
    {
        List<Func<bool>> LoadingTasks { get; }
    }
}
