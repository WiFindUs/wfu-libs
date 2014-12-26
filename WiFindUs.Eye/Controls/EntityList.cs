using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WiFindUs.Controls;

namespace WiFindUs.Eye.Controls
{
    public class EntityList : ThemedFlowLayoutPanel
    {
        private ISelectableEntityGroup selectionGroup = null;
        private ISelectableEntityGroup defaultSelectionGroup = new SelectableEntityGroup();
        private List<ISelectableEntity> entities = new List<ISelectableEntity>();
        
        public ISelectableEntityGroup SelectionGroup
        {
            get
            {
                return selectionGroup ?? defaultSelectionGroup;
            }
            set
            {
                if (entities.Count > 0)
                    throw new InvalidOperationException("You can only change the selection group of an entity list when it is empty!");
                ISelectableEntityGroup newValue = value == defaultSelectionGroup ? null : value;
                if (selectionGroup == newValue)
                    return;
                selectionGroup = newValue;
                foreach (ISelectableEntity entity in entities)
                    entity.SelectionGroup = SelectionGroup;
            }
        }
        
        public EntityList()
        {
            SuspendLayout();
            WrapContents = false;
            FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            ResumeLayout(true);
        }

        protected override void OnControlAdded(ControlEventArgs e)
        {
            base.OnControlAdded(e);

            EntityListItem elc = e.Control as EntityListItem;
            if (elc == null)
                return;
            if (entities.Contains(elc.Entity))
                throw new ArgumentOutOfRangeException("e", "An EntityListItem representing that entity is already present in this EntityList!");
            elc.Theme = Theme;
            elc.Width = ClientRectangle.Width - 1;
            entities.Add(elc.Entity);
            elc.Entity.SelectionGroup = SelectionGroup;
        }

        protected override void OnControlRemoved(ControlEventArgs e)
        {
            base.OnControlRemoved(e);
            
            EntityListItem elc = e.Control as EntityListItem;
            if (elc == null)
                return;

            entities.Remove(elc.Entity);
            if (elc.Entity.SelectionGroup == defaultSelectionGroup)
                elc.Entity.SelectionGroup = null;
        }
    }
}
