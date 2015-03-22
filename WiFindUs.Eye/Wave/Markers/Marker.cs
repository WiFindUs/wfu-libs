using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WaveEngine.Framework;
using WaveEngine.Framework.Graphics;
using WaveEngine.Framework.Physics3D;

namespace WiFindUs.Eye.Wave.Markers
{
    public abstract class Marker : Behavior
    {
        protected const float MAX_SPIN_RATE = 5.0f;
        private MapScene scene;
        private Transform3D transform;

        /////////////////////////////////////////////////////////////////////
        // PROPERTIES
        /////////////////////////////////////////////////////////////////////

        public MapScene Scene
        {
            get { return scene; }
        }

        public Transform3D Transform3D
        {
            get { return transform; }
        }

        public abstract BoxCollider BoxCollider { get; }

        public abstract bool Selected { get; set; }

        /////////////////////////////////////////////////////////////////////
        // PROTECTED METHODS
        /////////////////////////////////////////////////////////////////////

        protected override void Initialize()
        {
            base.Initialize();
            scene = Owner.Scene as MapScene;
            transform = Owner.FindComponent<Transform3D>();
        }
    }
}
