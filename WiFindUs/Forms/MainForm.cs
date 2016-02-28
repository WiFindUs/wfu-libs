using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Windows.Forms;

namespace WiFindUs.Forms
{
    public class MainForm : BaseForm, ISplashLoader
	{
		private static volatile bool closed = false;
		
		/////////////////////////////////////////////////////////////////////
		// PROPERTIES
		/////////////////////////////////////////////////////////////////////

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public virtual List<Func<bool>> LoadingTasks
		{
			get { return new List<Func<bool>>(); }
		}

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		protected override bool ShowWithoutActivation
		{
			get { return false; }
		}

		/// <summary>
		/// Global flag to test if the application's main form has been closed.
		/// </summary>
		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public static bool HasClosed
		{
			get { return closed; }
		}

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		protected virtual bool AutoApplyConfigState
		{
			get { return true; }
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
			if (WFUApplication.MainForm != null)
				throw new InvalidOperationException("You cannot instantiate more than one MainForm");
			closed = false;
			WFUApplication.MainForm = this;

			ShowInTaskbar = false;
#if DEBUG
			Debugger.T("exit");
#endif
		}

		/////////////////////////////////////////////////////////////////////
		// PUBLIC METHODS
		/////////////////////////////////////////////////////////////////////

		public static void SpinLock(Object lockObject, int timeout, Action action)
		{
			bool done = false;
			while (!done && !closed)
			{
				if (Monitor.TryEnter(lockObject, timeout))
				{
					try
					{
						action();
					}
					finally
					{
						Monitor.Exit(lockObject);
						done = true;
					}
				}
			}
		}

		public static void SafeSleep(int ms)
		{
			while (ms > 0 && !HasClosed)
			{
				int wait = Math.Min(ms, 100);
				Thread.Sleep(wait);
				ms -= wait;
			}
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
			if (AutoApplyConfigState)
				ApplyWindowStateFromConfig("main");
#if DEBUG
			Debugger.T("exit");
#endif
		}

		protected override void OnFormClosing(FormClosingEventArgs e)
		{
#if DEBUG
			Debugger.T("entry");
#endif
			base.OnFormClosing(e);
#if DEBUG
			Debugger.T("exit");
#endif
		}

		protected override void OnFormClosed(FormClosedEventArgs e)
		{
#if DEBUG
			Debugger.T("entry");
#endif
			closed = true;
			base.OnFormClosed(e);
#if DEBUG
			Debugger.T("exit");
#endif
		}

		protected override void OnDisposing()
		{
#if DEBUG
			Debugger.T("entry");
#endif
			base.OnDisposing();
#if DEBUG
			Debugger.T("exit");
#endif
		}

		/////////////////////////////////////////////////////////////////////
		// PRIVATE METHODS
		/////////////////////////////////////////////////////////////////////
	}
}
