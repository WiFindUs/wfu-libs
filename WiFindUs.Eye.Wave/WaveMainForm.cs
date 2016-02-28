using System;
using System.ComponentModel;

namespace WiFindUs.Eye.Wave
{
    public class WaveMainForm : EyeMainForm
	{
		private Map3D map3D = null;
		private static bool runApplicationCalled = false;

		/////////////////////////////////////////////////////////////////////
		// PROPERTIES
		/////////////////////////////////////////////////////////////////////

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool DebugMode
		{
			get { return map3D == null ? false : map3D.DebugMode; }
			set
			{
				if (map3D == null || value == map3D.DebugMode)
					return;
				map3D.DebugMode = value;
				OnDebugModeChanged();
			}
		}

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public Map3D Map3D
		{
			get { return map3D; }
			internal set
			{
				if (map3D != null || value == null)
					return;
#if DEBUG
				Debugger.T("entry");
#endif
				map3D = value;
				map3D.SceneStarted += OnMapSceneStarted;
				if (FirstShown)
					map3D.StartMapApplication(); //no-op if called already
#if DEBUG
				Debugger.T("exit");
#endif
			}
		}

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public static bool RunApplicationCalled
		{
			get { return runApplicationCalled; }
			internal set { runApplicationCalled = value; }
		}

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public override bool AutoStartPacketListeners
		{
			get { return false; }
		}

		/////////////////////////////////////////////////////////////////////
		// CONSTRUCTORS
		/////////////////////////////////////////////////////////////////////

		public WaveMainForm()
		{

		}

		/////////////////////////////////////////////////////////////////////
		// PUBLIC METHODS
		/////////////////////////////////////////////////////////////////////

		public static void RunApplication(WiFindUs.Forms.MainForm form)
		{
#if DEBUG
			Debugger.T("entry");
#endif
			if (WFUApplication.RunApplicationDefaultCalled)
			{
				Debugger.E("WFUApplication.RunApplicationDefault has already been called!");
				return;
			}
			else if (WaveMainForm.RunApplicationCalled)
			{
				Debugger.E("WaveMainForm.RunApplication has already been called!");
				return;
			}

			bool error = false;
			WaveMainForm mapForm = null;

			mapForm = form as WaveMainForm;
			if (mapForm == null)
				error = Debugger.E("The supplied MainForm type (" + form.GetType().FullName + ") does not inherit from WaveMainForm!");
			else if (mapForm.Map3D == null)
				error = Debugger.E("The application's WaveMainForm has not instantiated a Map3D!");

			if (error)
			{
				WFUApplication.RunApplicationDefault(form);
				return;
			}

			RenderLoop.Run(form, () =>
			{
				if (mapForm.map3D != null)
					mapForm.map3D.Render();
			});
#if DEBUG
			Debugger.T("exit");
#endif
		}

		/////////////////////////////////////////////////////////////////////
		// PROTECTED METHODS
		/////////////////////////////////////////////////////////////////////

		protected override void OnFirstShown(EventArgs e)
		{
#if DEBUG
			Debugger.T("entry");
#endif
			base.OnFirstShown(e);
			if (IsDesignMode)
				return;
			if (map3D != null)
				map3D.StartMapApplication(); //no-op if called already
#if DEBUG
			Debugger.T("exit");
#endif
		}

		protected virtual void OnMapSceneStarted(MapScene scene)
		{
#if DEBUG
			DebugMode = true;
#endif
			if (ListenForPackets)
				StartPacketListeners();
		}

		protected virtual void OnDebugModeChanged()
		{

		}
	}
}
