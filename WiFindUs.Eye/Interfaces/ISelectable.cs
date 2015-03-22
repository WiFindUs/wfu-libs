using System;

namespace WiFindUs.Eye
{
    public interface ISelectable
    {
        bool Selected { get; set; }
        event Action<ISelectable> SelectedChanged;
        ISelectableGroup SelectionGroup { get; set; }
    }
}
