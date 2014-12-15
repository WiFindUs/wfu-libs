using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WaveEngine.Common;
using WaveEngine.Framework;
using WaveEngine.Framework.Services;
using WiFindUs.Controls;

namespace WiFindUs.Eye.Wave
{
    public class MapGame : Game
    {
        public event Action<MapScene> SceneStarted;
        private MapScene scene;
        private MapControl hostControl;
       
        public MapScene Scene
        {
            get
            {
                return scene;
            }
        }

        public MapControl HostControl
        {
            get { return hostControl; }
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

        private void scene_SceneStarted(MapScene obj)
        {
            if (SceneStarted != null)
                SceneStarted(scene);
        }
    }
}
