using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WiFindUs.Eye
{
    public interface ISelectableProxy
    {
        ISelectable Selectable { get; }
    }
}
