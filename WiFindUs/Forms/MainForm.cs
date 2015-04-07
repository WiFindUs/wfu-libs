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
#if DEBUG
			Debugger.T("entry");
#endif
			if (IsDesignMode)
				return;

			ShowInTaskbar = false;
#if DEBUG
			Debugger.T("exit");
#endif
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
#if DEBUG
			Debugger.T("entry");
#endif
			base.OnFirstShown(e);
			ApplyWindowStateFromConfig("main");
#if DEBUG
			Debugger.T("exit");
#endif
		}

		/////////////////////////////////////////////////////////////////////
		// PRIVATE METHODS
		/////////////////////////////////////////////////////////////////////
	}
}
