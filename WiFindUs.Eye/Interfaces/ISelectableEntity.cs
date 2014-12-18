using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WiFindUs.Eye
{
    public interface ISelectableEntity
    {
        bool Selected { get; set; }
        event Action<ISelectableEntity> SelectedChanged;
    }
}
