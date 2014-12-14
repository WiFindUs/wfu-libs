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
        private MapScene scene;
       
        public MapScene Scene
        {
            get
            {
                return scene;
            }
        }

        
        public override void Initialize(IApplication application)
        {
            base.Initialize(application);
            scene = new MapScene();
            scene.Initialize(WaveServices.GraphicsDevice);

            ScreenContext sc = new ScreenContext(scene);
            WaveServices.ScreenContextManager.To(sc);
        }
    }
}
