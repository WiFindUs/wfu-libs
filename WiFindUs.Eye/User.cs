using System;

namespace WiFindUs.Eye
{
	public partial class User : SelectableEntity
	{
		public static event Action<User> OnUserLoaded;
		public event Action<User> OnUserTypeChanged;
		public event Action<User> OnUserFirstNameChanged;
		public event Action<User> OnUserMiddleNameChanged;
		public event Action<User> OnUserLastNameChanged;
		private bool loaded = false;

		/////////////////////////////////////////////////////////////////////
		// PROPERTIES
		/////////////////////////////////////////////////////////////////////

		public string FullName
		{
			get
			{
				return String.Format("{0} {1}{2}",
					NameFirst, (NameMiddle.Length == 0 ? "" : NameMiddle + " "), NameLast);
			}
		}

		/////////////////////////////////////////////////////////////////////
		// PUBLIC METHODS
		/////////////////////////////////////////////////////////////////////

		public override string ToString()
		{
			return String.Format("User[{0:X}] {1}", ID, FullName);
		}

		/////////////////////////////////////////////////////////////////////
		// PRIVATE METHODS
		/////////////////////////////////////////////////////////////////////

		partial void OnLoaded()
		{
			loaded = true;
			Debugger.V(this.ToString() + " loaded.");
			if (OnUserLoaded != null)
				OnUserLoaded(this);
		}

		partial void OnNameFirstChanged()
		{
			if (OnUserFirstNameChanged != null)
				OnUserFirstNameChanged(this);
		}

		partial void OnNameMiddleChanged()
		{
			if (OnUserMiddleNameChanged != null)
				OnUserMiddleNameChanged(this);
		}

		partial void OnNameLastChanged()
		{
			if (OnUserLastNameChanged != null)
				OnUserLastNameChanged(this);
		}

		partial void OnTypeChanged()
		{
			if (OnUserTypeChanged != null)
				OnUserTypeChanged(this);
		}

		public bool Loaded
		{
			get { return loaded; }
		}
	}
}
