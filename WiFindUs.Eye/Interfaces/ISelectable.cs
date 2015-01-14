using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WiFindUs.Eye
{
    public interface ISelectable
    {
        bool Selected { get; set; }
        event Action<ISelectable> SelectedChanged;
        ISelectableGroup SelectionGroup { get; set; }
    }
}
