using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using WiFindUs.Controls;
using WiFindUs.Extensions;

namespace WiFindUs.Eye.Controls
{
	public class EntityList : ThemedFlowLayoutPanel
	{
		private ISelectableGroup selectionGroup = null;
		private ISelectableGroup defaultSelectionGroup = new SelectableEntityGroup();
		private List<ISelectable> entities = new List<ISelectable>();

		public ISelectableGroup SelectionGroup
		{
			get
			{
				return selectionGroup ?? defaultSelectionGroup;
			}
			set
			{
				if (entities.Count > 0)
					throw new InvalidOperationException("You can only change the selection group of an entity list when it is empty!");
				ISelectableGroup newValue = value == defaultSelectionGroup ? null : value;
				if (selectionGroup == newValue)
					return;
				selectionGroup = newValue;
				foreach (ISelectable entity in entities)
					entity.SelectionGroup = SelectionGroup;
			}
		}

		public EntityList()
		{
			FlowDirection = FlowDirection.TopDown;
			AutoScroll = true;
			VScroll = true;
			WrapContents = false;
		}

		protected override void OnResize(EventArgs eventargs)
		{
			base.OnResize(eventargs);
			ResizeEntityItems();
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
			entities.Add(elc.Entity);
			elc.Entity.SelectionGroup = SelectionGroup;

			ResizeEntityItems();
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

		protected override void OnClick(EventArgs e)
		{
			base.OnClick(e);
			Focus();

			//do we have any entities?
			if (entities == null || entities.Count == 0)
				return;

			//check for any currently selected entities
			ISelectable[] selectedEntities = SelectionGroup.SelectedEntities;
			if (selectedEntities == null || selectedEntities.Length == 0)
				return;

			//check if any of the currently selected elements are in this list
			foreach (ISelectable entity in selectedEntities)
				if (entities.Contains(entity))
				{
					SelectionGroup.ClearSelection();
					break;
				}
		}

		private void ResizeEntityItems()
		{
			this.SuspendAllLayout();
			foreach (EntityListItem elitems in Controls.OfType<EntityListItem>())
				elitems.Width = ClientRectangle.Width - 1;
			this.ResumeAllLayout();

		}
	}
}
