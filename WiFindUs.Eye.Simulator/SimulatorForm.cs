using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WiFindUs.Eye.Simulator
{
	public partial class SimulatorForm : EyeMainForm
	{
		/////////////////////////////////////////////////////////////////////
		// PROPERTIES
		/////////////////////////////////////////////////////////////////////
		
		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		protected override DBMode DesiredDatabaseMode
		{
			get { return DBMode.Client; }
		}

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		protected override bool ListenForPackets
		{
			get { return false; }
		}

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		protected override uint MapLevels
		{
			get { return 1; }
		}

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		protected override bool AutomaticallyApplyConfigState
		{
			get { return false; }
		}

		/////////////////////////////////////////////////////////////////////
		// CONSTRUCTORS
		/////////////////////////////////////////////////////////////////////

		public SimulatorForm()
		{
			InitializeComponent();
		}

		/////////////////////////////////////////////////////////////////////
		// PROTECTED METHODS
		/////////////////////////////////////////////////////////////////////

		protected override void OnFirstShown(EventArgs e)
		{
			base.OnFirstShown(e);
			if (IsDesignMode)
				return;
			if (Map.ThreadlessState)
				Map.Load();
		}
	}


}
