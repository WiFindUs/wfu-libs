﻿using System;

namespace WiFindUs.Eye.Wave.Adapter
{
    public class MapApplication : FormApplication
	{
		public event Action<MapScene> SceneStarted;
		public event Action<MapApplication> ScreenResized;
		private MapGame game;
		private Map3D hostControl;

		/////////////////////////////////////////////////////////////////////
		// PROPERTIES
		/////////////////////////////////////////////////////////////////////

		public MapScene Scene
		{
			get { return game == null ? null : game.Scene; }
		}

		public bool DebugMode
		{
			get { return Scene == null ? false : Scene.DebugMode; }
			set
			{
				if (Scene == null)
					return;
				Scene.DebugMode = value;
			}
		}

		/////////////////////////////////////////////////////////////////////
		// CONSTRUCTORS
		/////////////////////////////////////////////////////////////////////

		public MapApplication(Map3D hostControl, int width, int height)
			: base(width, height)
		{
			if (hostControl == null)
				throw new ArgumentNullException("hostControl", "MapApplication cannot be instantiated outside of a host MapControl.");
			this.hostControl = hostControl;
		}

		/////////////////////////////////////////////////////////////////////
		// PUBLIC METHODS
		/////////////////////////////////////////////////////////////////////

		public override void Initialize()
		{
			game = new MapGame(hostControl);
			game.SceneStarted += scene_SceneStarted;
			game.Initialize(this);
		}

		public override void Update(TimeSpan elapsedTime)
		{
			if (game != null && !game.HasExited)
				game.UpdateFrame(elapsedTime);
		}

		public override void Draw(TimeSpan elapsedTime)
		{
			if (game != null && !game.HasExited)
				game.DrawFrame(elapsedTime);
		}

		/// <summary>
		/// Called when [activated].
		/// </summary>
		public override void OnActivated()
		{
			base.OnActivated();
			if (game != null)
				game.OnActivated();
		}

		/// <summary>
		/// Called when [deactivate].
		/// </summary>
		public override void OnDeactivate()
		{
			base.OnDeactivate();
			if (game != null)
				game.OnDeactivated();
		}

		public override void ResizeScreen(int width, int height)
		{
			base.ResizeScreen(width, height);
			if (ScreenResized != null)
				ScreenResized(this);
		}

		/////////////////////////////////////////////////////////////////////
		// PRIVATE METHODS
		/////////////////////////////////////////////////////////////////////

		private void scene_SceneStarted(MapScene obj)
		{
			if (SceneStarted != null)
				SceneStarted(obj);
		}
	}
}

