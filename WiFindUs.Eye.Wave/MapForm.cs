// File created: 3/26/2015 3:17:34 PM by marzer

using System;
using WiFindUs.Extensions;
using WiFindUs.Eye.Extensions;
using WiFindUs.Eye.Wave.Adapter;
using WiFindUs.Eye.Wave.Controls;
using WiFindUs.Forms;

namespace WiFindUs.Eye.Wave
{
	public class MapForm : BaseForm
	{
		private MapControl map;

		/////////////////////////////////////////////////////////////////////
		// PROPERTIES
		/////////////////////////////////////////////////////////////////////

		public MapControl Map
		{
			get { return map; }
			set
			{
				if (value == map)
					return;
				this.SuspendAllLayout();
				if (map != null)
				{
					map.Parent = null;
					map = null;
				}
				map = value;
				if (map != null)
				{
					map.Parent = this;
					map.Dock = System.Windows.Forms.DockStyle.Fill;
					map = value;
				}
				this.ResumeAllLayout();
			}
		}

		/////////////////////////////////////////////////////////////////////
		// CONSTRUCTORS
		/////////////////////////////////////////////////////////////////////

		public MapForm()
			: base()
		{

		}
	}
}