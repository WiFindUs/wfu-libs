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
        ISelectableEntity[] SelectedEntities { get; }
        void AddSelectableEntity(params ISelectableEntity[] entities);
        void RemoveSelectableEntity(params ISelectableEntity[] entities);

        //entire collection selection
        void SelectAllEntities();
        void ClearEntitySelection();
        void InvertEntitySelection();

        //adding to selection
        void AddToEntitySelection(params ISelectableEntity[] entities);
        void AddToEntitySelection(IEnumerable<ISelectableEntity> entities);

        //removing from selection
        void RemoveFromEntitySelection(params ISelectableEntity[] entities);
        void RemoveFromEntitySelection(IEnumerable<ISelectableEntity> entities);
        
        //setting selection
        void SetEntitySelection(params ISelectableEntity[] entity);
        void SetEntitySelection(IEnumerable<ISelectableEntity> entities);

        //toggling selection
        void ToggleEntitySelection(params ISelectableEntity[] entity);
        void ToggleEntitySelection(IEnumerable<ISelectableEntity> entities);
    }
}
