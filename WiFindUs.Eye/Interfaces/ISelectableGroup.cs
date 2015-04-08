using System;
using System.Collections.Generic;

namespace WiFindUs.Eye
{
	public interface ISelectableGroup
	{
		//selection list logic
		ISelectable[] Entities { get; }
		ISelectable[] SelectedEntities { get; }
		void Add(params ISelectable[] entities);
		void Remove(params ISelectable[] entities);
		void Clear();
		event Action<ISelectableGroup> SelectionChanged;
		void NotifySelectionChanged(ISelectable sender);
		bool CaptureNotifies { get; set; }
		T[] EntitiesByType<T>();
		T[] SelectedEntitiesByType<T>();

		//entire collection selection
		void SelectAll();
		void ClearSelection();
		void InvertSelection();

		//adding to selection
		void AddToSelection(params ISelectable[] entities);
		void AddToSelection(IEnumerable<ISelectable> entities);

		//removing from selection
		void RemoveFromSelection(params ISelectable[] entities);
		void RemoveFromSelection(IEnumerable<ISelectable> entities);

		//setting selection
		void SetSelection(params ISelectable[] entity);
		void SetSelection(IEnumerable<ISelectable> entities);

		//toggling selection
		void ToggleSelection(params ISelectable[] entity);
		void ToggleSelection(IEnumerable<ISelectable> entities);
	}
}
