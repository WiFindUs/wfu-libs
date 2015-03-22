using System;

namespace WiFindUs.Eye
{
	public class SelectableEntity : ISelectable
	{
		private bool selected = false;
		private ISelectableGroup group = null;
		public event Action<ISelectable> SelectedChanged;

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
				if (group != null)
					group.NotifySelectionChanged(this);
			}
		}

		public ISelectableGroup SelectionGroup
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
