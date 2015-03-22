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

namespace WiFindUs.Eye.Wave.Adapter
{
    public class MapApplication : FormApplication
    {
        public event Action<MapScene> SceneStarted;
        public event Action<MapApplication> ScreenResized;
        private MapGame game;
        private MapControl hostControl;

        /////////////////////////////////////////////////////////////////////
        // PROPERTIES
        /////////////////////////////////////////////////////////////////////

        public MapScene Scene
        {
            get
            {
                return game == null ? null : game.Scene;
            }
        }

        public MapControl HostControl
        {
            get { return hostControl; }
        }

        /////////////////////////////////////////////////////////////////////
        // CONSTRUCTORS
        /////////////////////////////////////////////////////////////////////

        public MapApplication(MapControl hostControl, int width, int height)
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
            game = new MapGame(this);
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

        public void CancelThreads()
        {
            if (game != null)
                game.CancelThreads();
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

