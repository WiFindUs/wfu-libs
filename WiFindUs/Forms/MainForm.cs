using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace WiFindUs.Forms
{
	public class MainForm : BaseForm, ISplashLoader
	{
		/////////////////////////////////////////////////////////////////////
		// PROPERTIES
		/////////////////////////////////////////////////////////////////////

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public virtual List<Func<bool>> LoadingTasks
		{
			get { return new List<Func<bool>>(); }
		}

		protected override bool ShowWithoutActivation
		{
			get { return false; }
		}

		/////////////////////////////////////////////////////////////////////
		// CONSTRUCTORS
		/////////////////////////////////////////////////////////////////////

		public MainForm()
		{
			if (IsDesignMode)
				return;

			ShowInTaskbar = false;
		}

		/////////////////////////////////////////////////////////////////////
		// PROTECTED METHODS
		/////////////////////////////////////////////////////////////////////

		protected override void SetVisibleCore(bool value)
		{
			base.SetVisibleCore(IsDesignMode || WFUApplication.SplashLoadingFinished ? value : false);
		}

		protected override void OnFirstShown(EventArgs e)
		{
			base.OnFirstShown(e);
			ApplyWindowStateFromConfig("main");
		}

		/////////////////////////////////////////////////////////////////////
		// PRIVATE METHODS
		/////////////////////////////////////////////////////////////////////
	}
}
