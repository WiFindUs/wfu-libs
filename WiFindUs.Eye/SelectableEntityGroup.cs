using System;
using System.Collections.Generic;
using System.Linq;

namespace WiFindUs.Eye
{
	public class SelectableEntityGroup : ISelectableGroup
	{
		private List<ISelectable> managedEntities = new List<ISelectable>();
		private int capturedChangeNotifications = 0;
		private bool capturingNotifications = false;

		/////////////////////////////////////////////////////////////////////
		// PROPERTIES
		/////////////////////////////////////////////////////////////////////

		public event Action<ISelectableGroup> SelectionChanged;

		public bool CaptureNotifies
		{
			get { return capturingNotifications; }
			set
			{
				if (value == capturingNotifications)
					return;

				if (value)
					capturedChangeNotifications = 0;
				else
				{
					if (capturedChangeNotifications > 0 && SelectionChanged != null)
						SelectionChanged(this);
				}

				capturingNotifications = value;
			}
		}

		public ISelectable[] Entities
		{
			get
			{
				return managedEntities.ToArray();
			}
		}

		public ISelectable[] SelectedEntities
		{
			get
			{
				List<ISelectable> selectedEntities = new List<ISelectable>();
				foreach (ISelectable entity in managedEntities)
					if (entity.Selected)
						selectedEntities.Add(entity);
				return selectedEntities.ToArray();
			}
		}

		/////////////////////////////////////////////////////////////////////
		// PUBLIC METHODS
		/////////////////////////////////////////////////////////////////////

		public void NotifySelectionChanged(ISelectable sender)
		{
			if (sender == null || sender.SelectionGroup != this)
				return;
			if (CaptureNotifies)
				capturedChangeNotifications++;
			else if (SelectionChanged != null)
				SelectionChanged(this);
		}

		public void Add(params ISelectable[] entities)
		{
			if (entities == null || entities.Length == 0)
				return;
			foreach (ISelectable entity in entities)
			{
				if (entity == null)
					continue;
				entity.Selected = false;
				entity.SelectionGroup = this;
				if (!managedEntities.Contains(entity))
					managedEntities.Add(entity);
			}
		}

		public void Remove(params ISelectable[] entities)
		{
			if (entities == null || entities.Length == 0)
				return;
			foreach (ISelectable entity in entities)
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
			CaptureNotifies = true;
			foreach (ISelectable entity in managedEntities)
				entity.Selected = true;
			CaptureNotifies = false;
		}

		public void ClearSelection()
		{
			CaptureNotifies = true;
			foreach (ISelectable entity in managedEntities)
				entity.Selected = false;
			CaptureNotifies = false;
		}

		public void InvertSelection()
		{
			CaptureNotifies = true;
			foreach (ISelectable entity in managedEntities)
				entity.Selected = !entity.Selected;
			CaptureNotifies = false;
		}

		public void AddToSelection(params ISelectable[] entities)
		{
			if (entities == null || entities.Length == 0)
				return;
			CaptureNotifies = true;
			foreach (ISelectable entity in entities)
			{
				if (entity == null || entity.SelectionGroup != this)
					continue;
				entity.Selected = true;
			}
			CaptureNotifies = false;
		}

		public void AddToSelection(IEnumerable<ISelectable> entities)
		{
			AddToSelection(entities.ToArray());
		}

		public void RemoveFromSelection(params ISelectable[] entities)
		{
			if (entities == null || entities.Length == 0)
				return;
			CaptureNotifies = true;
			foreach (ISelectable entity in entities)
			{
				if (entity == null || entity.SelectionGroup != this)
					continue;
				entity.Selected = false;
			}
			CaptureNotifies = false;
		}

		public void RemoveFromSelection(IEnumerable<ISelectable> entities)
		{
			RemoveFromSelection(entities.ToArray());
		}

		public void SetSelection(params ISelectable[] entities)
		{
			if (entities == null || entities.Length == 0)
			{
				ClearSelection();
				return;
			}

			CaptureNotifies = true;
			List<ISelectable> leftOver = new List<ISelectable>(managedEntities);
			foreach (ISelectable entity in entities)
			{
				if (entity == null || entity.SelectionGroup != this)
					continue;
				entity.Selected = true;
				leftOver.Remove(entity);
			}

			foreach (ISelectable entity in leftOver)
				entity.Selected = false;
			CaptureNotifies = false;
		}

		public void SetSelection(IEnumerable<ISelectable> entities)
		{
			SetSelection(entities.ToArray());
		}

		public void ToggleSelection(params ISelectable[] entities)
		{
			if (entities == null || entities.Length == 0)
				return;
			CaptureNotifies = true;
			foreach (ISelectable entity in entities)
			{
				if (entity == null || entity.SelectionGroup != this)
					continue;
				entity.Selected = !entity.Selected;
			}
			CaptureNotifies = false;
		}

		public void ToggleSelection(IEnumerable<ISelectable> entities)
		{
			ToggleSelection(entities.ToArray());
		}
	}
}
