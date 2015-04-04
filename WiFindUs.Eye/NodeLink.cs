﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WiFindUs.Eye
{
	public partial class NodeLink
	{
		public static event Action<NodeLink> OnNodeLinkLoaded;
		public event Action<NodeLink> OnNodeLinkActiveChanged;
		public event Action<NodeLink> OnNodeLinkSpeedChanged;
		public event Action<NodeLink> OnNodeLinkSignalStrengthChanged;

		private bool loaded = false;

		/////////////////////////////////////////////////////////////////////
		// PROPERTIES
		/////////////////////////////////////////////////////////////////////

		public bool Loaded
		{
			get { return loaded; }
		}

		/////////////////////////////////////////////////////////////////////
		// PRIVATE METHODS
		/////////////////////////////////////////////////////////////////////

		partial void OnLoaded()
		{
			loaded = true;
			Debugger.V(this.ToString() + " loaded.");
			if (OnNodeLinkLoaded != null)
				OnNodeLinkLoaded(this);
		}

		partial void OnActiveChanged()
		{
			if (OnNodeLinkActiveChanged != null)
				OnNodeLinkActiveChanged(this);
		}

		partial void OnLinkSpeedChanged()
		{
			if (OnNodeLinkSpeedChanged != null)
				OnNodeLinkSpeedChanged(this);
		}

		partial void OnSignalStrengthChanged()
		{
			if (OnNodeLinkSignalStrengthChanged != null)
				OnNodeLinkSignalStrengthChanged(this);
		}
	}
}