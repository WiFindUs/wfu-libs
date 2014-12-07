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
    public class MapGame : Game, IThemeable
    {
        private MapScene scene;
       
        public Theme Theme
        {
            get
            {
                return scene == null ? null : scene.Theme;
            }
            set
            {
                if (scene != null)
                    scene.Theme = value;
            }
        }

        public ILocation CenterLocation
        {
            get
            {
                return scene == null ? null : scene.CenterLocation;
            }
            set
            {
                if (scene == null)
                    return;
                scene.CenterLocation = value;
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

        public void CancelThreads()
        {
            if (scene != null)
                scene.CancelThreads();
        }
    }
}
