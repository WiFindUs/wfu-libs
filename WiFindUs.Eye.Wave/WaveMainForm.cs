using System;
using System.ComponentModel;
using WiFindUs.Eye.Extensions;
using WiFindUs.Eye.Wave.Adapter;
using WiFindUs.Eye.Wave.Controls;

namespace WiFindUs.Eye.Wave
{
    public class WaveMainForm : EyeMainForm, IMapForm
    {
        private MapControl map;

        /////////////////////////////////////////////////////////////////////
        // PROPERTIES
        /////////////////////////////////////////////////////////////////////

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
                map.Theme = Theme;
                map.ApplicationStarting += MapApplicationStarting;
                map.SceneStarted += MapSceneStarted;
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

        public void RenderMap()
        {
            if (Map != null)
                Map.Render();
        }

        /////////////////////////////////////////////////////////////////////
        // PROTECTED METHODS
        /////////////////////////////////////////////////////////////////////

        protected override void OnFirstShown(EventArgs e)
        {
            base.OnFirstShown(e);
            if (IsDesignMode)
                return;

            //start map scene
            if (Map != null)
                Map.StartMapApplication();
        }

        protected override void OnDisposing()
        {
            if (Map != null)
            {
                Map.CancelThreads();
                Map.Dispose();
            }
            base.OnDisposing();
        }

        protected virtual void MapSceneStarted(MapScene obj)
        {
            ILocation location = WFUApplication.Config.Get("map.center", (ILocation)null);
            if (location == null)
                Debugger.E("Could not parse map.center from config files!");
            else
                obj.CenterLocation = location;
            Map.Scene.Theme = Theme;
        }

        /////////////////////////////////////////////////////////////////////
        // PRIVATE METHODS
        /////////////////////////////////////////////////////////////////////

        private void MapApplicationStarting(MapControl map)
        {
            if (WFUApplication.Config != null)
                map.BackBufferScale = WFUApplication.Config.Get("map.resolution_scale", 1.0f);
        }

    }
}
