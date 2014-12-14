using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WaveEngine.Adapter;
using WaveEngine.Common.Graphics;
using WaveEngine.Common.Math;
using WaveEngine.Framework.Graphics;
using WaveEngine.Framework.Services;
using WiFindUs.Controls;

namespace WiFindUs.Eye.Wave
{
    public class MapApplication : FormApplication
    {
        MapGame game;
        MapControl hostControl;

        public MapScene Scene
        {
            get
            {
                return game == null ? null : game.Scene;
            }
        }

        public MapApplication(MapControl hostControl, int width, int height)
            : base(width, height)
        {
            this.hostControl = hostControl;
        }

        public override void Initialize()
        {
            game = new MapGame(hostControl);
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
    }
}

