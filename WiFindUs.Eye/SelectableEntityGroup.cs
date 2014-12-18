using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WiFindUs.Eye
{
    public class SelectableEntityGroup : ISelectableEntityGroup
    {
        private List<ISelectableEntity> managedEntities = new List<ISelectableEntity>();
        
        public ISelectableEntity[] SelectedEntities
        {
            get
            {
                List<ISelectableEntity> selectedEntities = new List<ISelectableEntity>();
                foreach (ISelectableEntity entity in managedEntities)
                    if (entity.Selected)
                        selectedEntities.Add(entity);
                return selectedEntities.ToArray();
            }
        }

        public void AddSelectableEntity(params ISelectableEntity[] entities)
        {
            if (entities == null || entities.Length == 0)
                return;
            foreach (ISelectableEntity entity in entities)
            {
                if (entity == null)
                    continue;
                entity.SelectionGroup = this;
                if (!managedEntities.Contains(entity))
                    managedEntities.Add(entity);
            }
        }

        public void RemoveSelectableEntity(params ISelectableEntity[] entities)
        {
            if (entities == null || entities.Length == 0)
                return;
            foreach (ISelectableEntity entity in entities)
            {
                if (entity == null)
                    continue;
                entity.Selected = false;
                entity.SelectionGroup = null;
                if (managedEntities.Contains(entity))
                    managedEntities.Remove(entity);
            }
        }

        public void SelectAllEntities()
        {
            foreach (ISelectableEntity entity in managedEntities)
                entity.Selected = true;
        }

        public void ClearEntitySelection()
        {
            foreach (ISelectableEntity entity in managedEntities)
                entity.Selected = false;
        }

        public void InvertEntitySelection()
        {
            foreach (ISelectableEntity entity in managedEntities)
                entity.Selected = !entity.Selected;
        }

        public void AddToEntitySelection(params ISelectableEntity[] entities)
        {
            if (entities == null || entities.Length == 0)
                return;
            foreach (ISelectableEntity entity in entities)
            {
                if (entity == null || entity.SelectionGroup != this)
                    continue;
                entity.Selected = true;
            }
        }

        public void AddToEntitySelection(IEnumerable<ISelectableEntity> entities)
        {
            AddToEntitySelection(entities.ToArray());
        }

        public void RemoveFromEntitySelection(params ISelectableEntity[] entities)
        {
            if (entities == null || entities.Length == 0)
                return;
            foreach (ISelectableEntity entity in entities)
            {
                if (entity == null || entity.SelectionGroup != this)
                    continue;
                entity.Selected = false;
            }
        }

        public void RemoveFromEntitySelection(IEnumerable<ISelectableEntity> entities)
        {
            RemoveFromEntitySelection(entities.ToArray());
        }

        public void SetEntitySelection(params ISelectableEntity[] entities)
        {
            if (entities == null || entities.Length == 0)
            {
                ClearEntitySelection();
                return;
            }

            List<ISelectableEntity> leftOver = new List<ISelectableEntity>(managedEntities);
            foreach (ISelectableEntity entity in entities)
            {
                if (entity == null || entity.SelectionGroup != this)
                    continue;
                entity.Selected = true;
                leftOver.Remove(entity);
            }

            foreach (ISelectableEntity entity in leftOver)
                entity.Selected = false;
        }

        public void SetEntitySelection(IEnumerable<ISelectableEntity> entities)
        {
            SetEntitySelection(entities.ToArray());
        }

        public void ToggleEntitySelection(params ISelectableEntity[] entities)
        {
            if (entities == null || entities.Length == 0)
                return;
            foreach (ISelectableEntity entity in entities)
            {
                if (entity == null || entity.SelectionGroup != this)
                    continue;
                entity.Selected = !entity.Selected;
            }
        }

        public void ToggleEntitySelection(IEnumerable<ISelectableEntity> entities)
        {
            ToggleEntitySelection(entities.ToArray());
        }
    }
}
