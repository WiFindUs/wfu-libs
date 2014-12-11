using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WiFindUs.Eye
{
    public interface IUpdateable
    {
        bool TimedOut { get; }
        void CheckTimeout();
        long UpdateAge { get; }
        long Updated { get; }
    }
}
