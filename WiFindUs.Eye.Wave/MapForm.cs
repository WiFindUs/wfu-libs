// File created: 3/26/2015 3:17:34 PM by marzer

using WiFindUs.Extensions;
using WiFindUs.Forms;

namespace WiFindUs.Eye.Wave
{
    public class MapForm : BaseForm
	{
		private Map3D map;

		/////////////////////////////////////////////////////////////////////
		// PROPERTIES
		/////////////////////////////////////////////////////////////////////

		public Map3D Map
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
