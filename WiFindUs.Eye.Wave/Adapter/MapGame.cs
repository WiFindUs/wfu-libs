using System;

namespace WiFindUs.Eye.Wave.Adapter
{
    public class MapGame : Game
	{
		public event Action<MapScene> SceneStarted;
		private MapScene scene;
		private Map3D hostControl;

		public MapScene Scene
		{
			get { return scene; }
		}

		public Map3D HostControl
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

		public MapGame(Map3D hostControl)
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

		private void scene_SceneStarted(MapScene obj)
		{
			if (SceneStarted != null)
				SceneStarted(scene);
		}
	}
}
