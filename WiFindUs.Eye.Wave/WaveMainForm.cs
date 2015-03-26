using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using WaveEngine.Adapter.Win32;
using WiFindUs.Eye.Extensions;
using WiFindUs.Eye.Wave.Adapter;
using WiFindUs.Eye.Wave.Controls;

namespace WiFindUs.Eye.Wave
{
	public class WaveMainForm : EyeMainForm, IMapForm
	{
		private List<MapControl> maps = new List<MapControl>(4);

		/////////////////////////////////////////////////////////////////////
		// PROPERTIES
		/////////////////////////////////////////////////////////////////////

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool DebugMode
		{
			get { return maps.Count == 0 ? false : maps[0].DebugMode; }
			set
			{
				if (maps.Count == 0 || value == maps[0].DebugMode)
					return;
				for (int i = 0; i < maps.Count; i++)
					maps[i].DebugMode = value;
				OnDebugModeChanged();
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
				mapForm.RenderMap();
			});
		}

		public void RenderMap()
		{
			for (int i = 0; i < maps.Count; i++)
				maps[i].Render();
		}

		public void AddMapControl(MapControl mapControl)
		{
			if (mapControl == null || maps.Contains(mapControl))
				return;
			maps.Add(mapControl);
			OnMapControlAdded(mapControl);
			mapControl.Theme = Theme;
			mapControl.SceneStarted += OnMapSceneStarted;
			if (FirstShown)
				mapControl.StartMapApplication(); //no-op if called already
		}

		public void RemoveMapControl(MapControl mapControl)
		{
			if (mapControl == null)
				return;
			maps.Remove(mapControl);
			OnMapControlRemoved(mapControl);
			mapControl.SceneStarted -= OnMapSceneStarted;
		}

		/////////////////////////////////////////////////////////////////////
		// PROTECTED METHODS
		/////////////////////////////////////////////////////////////////////

		protected virtual void OnMapControlAdded(MapControl mapControl)
		{

		}

		protected virtual void OnMapControlRemoved(MapControl mapControl)
		{

		}

		protected override void OnFirstShown(EventArgs e)
		{
			base.OnFirstShown(e);
			if (IsDesignMode)
				return;

			//start map scenes
			for (int i = 0; i < maps.Count; i++)
				maps[i].StartMapApplication();  //no-op if called already
		}

		protected virtual void OnMapSceneStarted(MapScene scene)
		{

		}

		protected virtual void OnDebugModeChanged()
		{

		}

	}
}
