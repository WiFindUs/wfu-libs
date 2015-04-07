using System;
using WaveEngine.Common;
using WaveEngine.Framework;
using WaveEngine.Framework.Services;
using WiFindUs.Eye.Wave.Controls;

namespace WiFindUs.Eye.Wave.Adapter
{
	public class MapGame : Game
	{
		public event Action<MapScene> SceneStarted;
		private MapScene scene;
		private MapControl hostControl;

		public MapScene Scene
		{
			get { return scene; }
		}

		public MapControl HostControl
		{
			get { return hostControl; }
		}

		public bool DebugMode
		{
			get { return scene == null ? false : scene.DebugMode; }
			set
			{
				if (scene == null)
					return;
				scene.DebugMode = value;
			}
		}

		public MapGame(MapControl hostControl)
		{
			if (hostControl == null)
				throw new ArgumentNullException("hostControl", "MapGame cannot be instantiated outside of a host MapControl.");
			this.hostControl = hostControl;
		}

		public override void Initialize(IApplication application)
		{
			base.Initialize(application);
			scene = new MapScene(hostControl);
			scene.SceneStarted += scene_SceneStarted;
			scene.Initialize(WaveServices.GraphicsDevice);

			ScreenContext sc = new ScreenContext(scene);
			WaveServices.ScreenContextManager.To(sc);
		}

		public void CancelThreads()
		{
			if (scene != null)
				scene.CancelThreads();
		}

		private void scene_SceneStarted(MapScene obj)
		{
			if (SceneStarted != null)
				SceneStarted(scene);
		}
	}
}
