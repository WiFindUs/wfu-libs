using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WiFindUs.Eye
{
    public interface ISelectableEntityGroup
    {
        //selection list logic
        ISelectableEntity[] Entities { get; }
        ISelectableEntity[] SelectedEntities { get; }
        void Add(params ISelectableEntity[] entities);
        void Remove(params ISelectableEntity[] entities);
        void Clear();

        //entire collection selection
        void SelectAll();
        void ClearSelection();
        void InvertSelection();

        //adding to selection
        void AddToSelection(params ISelectableEntity[] entities);
        void AddToSelection(IEnumerable<ISelectableEntity> entities);

        //removing from selection
        void RemoveFromSelection(params ISelectableEntity[] entities);
        void RemoveFromSelection(IEnumerable<ISelectableEntity> entities);
        
        //setting selection
        void SetSelection(params ISelectableEntity[] entity);
        void SetSelection(IEnumerable<ISelectableEntity> entities);

        //toggling selection
        void ToggleSelection(params ISelectableEntity[] entity);
        void ToggleSelection(IEnumerable<ISelectableEntity> entities);
    }
}
