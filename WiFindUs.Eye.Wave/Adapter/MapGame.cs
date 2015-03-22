using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WaveEngine.Common;
using WaveEngine.Framework;
using WaveEngine.Framework.Services;
using WiFindUs.Controls;

namespace WiFindUs.Eye.Wave.Adapter
{
    public class MapGame : Game
    {
        public event Action<MapScene> SceneStarted;
        private MapScene scene;
        private MapApplication hostApplication;
       
        public MapScene Scene
        {
            get
            {
                return scene;
            }
        }

        public MapApplication HostApplication
        {
            get { return hostApplication; }
        }

        public MapControl HostControl
        {
            get { return hostApplication.HostControl; }
        }

        public MapGame(MapApplication hostApplication)
        {
            if (hostApplication == null)
                throw new ArgumentNullException("HostApplication", "MapGame cannot be instantiated outside of a host MapApplication.");
            this.hostApplication = hostApplication;
        }
        
        public override void Initialize(IApplication application)
        {
            base.Initialize(application);
            scene = new MapScene(this);
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
