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
		public event Action<User> OnUserDeviceChanged;
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

		public string ShortName
		{
			get { return String.Format("{0} {1}", NameFirst, NameLast.Length == 0 ? "" : NameLast.Substring(0, 1)); }
		}

		public bool Loaded
		{
			get { return loaded; }
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
			if (!loaded)
			{
				loaded = true;
				PropertyChanged += UserPropertyChanged;
				Debugger.V(this.ToString() + " loaded.");
				if (OnUserLoaded != null)
					OnUserLoaded(this);
			}
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

		private void UserPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			switch (e.PropertyName)
			{
				case "Device":
					if (OnUserDeviceChanged != null)
						OnUserDeviceChanged(this);
					break;
			}
		}
	}
}
