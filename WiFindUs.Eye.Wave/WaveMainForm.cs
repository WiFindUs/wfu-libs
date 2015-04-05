﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using WaveEngine.Adapter.Win32;
using WiFindUs.Eye.Extensions;
using WiFindUs.Eye.Wave.Adapter;
using WiFindUs.Eye.Wave.Controls;

namespace WiFindUs.Eye.Wave
{
	public class WaveMainForm : EyeMainForm
	{
		private MapControl map = null;

		/////////////////////////////////////////////////////////////////////
		// PROPERTIES
		/////////////////////////////////////////////////////////////////////

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool DebugMode
		{
			get { return map == null ? false : map.DebugMode; }
			set
			{
				if (map == null || value == map.DebugMode)
					return;
				map.DebugMode = value;
				OnDebugModeChanged();
			}
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public MapControl Map
		{
			get { return map; }
			protected set
			{
				if (map != null || value == null)
					return;
				map = value;
				map.SceneStarted += OnMapSceneStarted;
				if (FirstShown)
					map.StartMapApplication(); //no-op if called already
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
				if (mapForm.map != null)
					mapForm.map.Render();
			});
		}

		public void AddMapControl(MapControl mapControl)
		{

		}


		/////////////////////////////////////////////////////////////////////
		// PROTECTED METHODS
		/////////////////////////////////////////////////////////////////////

		protected override void OnFirstShown(EventArgs e)
		{
			base.OnFirstShown(e);
			if (IsDesignMode)
				return;
			if (map != null)
				map.StartMapApplication(); //no-op if called already
		}

		protected virtual void OnMapSceneStarted(MapScene scene)
		{

		}

		protected virtual void OnDebugModeChanged()
		{

		}

	}
}
