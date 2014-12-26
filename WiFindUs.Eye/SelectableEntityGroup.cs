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

        public ISelectableEntity[] Entities
        {
            get
            {
                return managedEntities.ToArray();
            }
        }

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

        public void Add(params ISelectableEntity[] entities)
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

        public void Remove(params ISelectableEntity[] entities)
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

        public void Clear()
        {
            Remove(managedEntities.ToArray());
        }

        public void SelectAll()
        {
            foreach (ISelectableEntity entity in managedEntities)
                entity.Selected = true;
        }

        public void ClearSelection()
        {
            foreach (ISelectableEntity entity in managedEntities)
                entity.Selected = false;
        }

        public void InvertSelection()
        {
            foreach (ISelectableEntity entity in managedEntities)
                entity.Selected = !entity.Selected;
        }

        public void AddToSelection(params ISelectableEntity[] entities)
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

        public void AddToSelection(IEnumerable<ISelectableEntity> entities)
        {
            AddToSelection(entities.ToArray());
        }

        public void RemoveFromSelection(params ISelectableEntity[] entities)
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

        public void RemoveFromSelection(IEnumerable<ISelectableEntity> entities)
        {
            RemoveFromSelection(entities.ToArray());
        }

        public void SetSelection(params ISelectableEntity[] entities)
        {
            if (entities == null || entities.Length == 0)
            {
                ClearSelection();
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

        public void SetSelection(IEnumerable<ISelectableEntity> entities)
        {
            SetSelection(entities.ToArray());
        }

        public void ToggleSelection(params ISelectableEntity[] entities)
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

        public void ToggleSelection(IEnumerable<ISelectableEntity> entities)
        {
            ToggleSelection(entities.ToArray());
        }
    }
}
