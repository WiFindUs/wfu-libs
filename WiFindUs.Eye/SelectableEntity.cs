using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WiFindUs.Eye
{
    public class SelectableEntity : ISelectableEntity
    {
        private bool selected = false;
        private ISelectableEntityGroup group = null;
        public event Action<ISelectableEntity> SelectedChanged;

        public bool Selected
        {
            get { return selected; }
            set
            {
                if (selected == value)
                    return;
                selected = value;
                if (SelectedChanged != null)
                    SelectedChanged(this);
            }
        }       

        public ISelectableEntityGroup SelectionGroup
        {
            get
            {
                return group;
            }
            set
            {
                if (group == value)
                    return;
                if (group != null)
                    group.Remove(this);
                group = value;
                if (group != null)
                    group.Add(this);
            }
        }
    }
}
