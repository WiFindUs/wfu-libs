using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using WaveEngine.Adapter.Win32;
using WiFindUs.Eye.Wave.Adapter;
using WiFindUs.Eye.Wave;

namespace WiFindUs.Eye.Wave
{
	public class WaveMainForm : EyeMainForm
	{
		private Map3D map3D = null;

		/////////////////////////////////////////////////////////////////////
		// PROPERTIES
		/////////////////////////////////////////////////////////////////////

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
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

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
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

		/////////////////////////////////////////////////////////////////////
		// CONSTRUCTORS
		/////////////////////////////////////////////////////////////////////

		public WaveMainForm()
		{

		}

		/////////////////////////////////////////////////////////////////////
		// PUBLIC METHODS
		/////////////////////////////////////////////////////////////////////

		public static void StartRenderLoop(WiFindUs.Forms.MainForm form)
		{
#if DEBUG
			Debugger.T("entry");
#endif
			WaveMainForm mapForm = form as WaveMainForm;
			if (mapForm == null)
			{
				String message = "The supplied MainForm type (" + form.GetType().FullName + ") does not inherit from WaveMainForm!";
				Debugger.E(message);
				MessageBox.Show(message + "\n\nThe application will now exit.", "WaveMainForm Initialization Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
		}

		protected virtual void OnDebugModeChanged()
		{

		}

	}
}
