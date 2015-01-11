using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WaveEngine.Common.Math;
using WaveEngine.Components.Graphics3D;
using WaveEngine.Framework;
using WaveEngine.Framework.Graphics;
using WaveEngine.Framework.Physics3D;

namespace WiFindUs.Eye.Wave
{
    public class NodeMarker : EntityMarker
    {
        private Node node;
        [RequiredComponent]
        private Transform3D transform3D;
        [RequiredComponent]
        private MaterialsMap materialsMap;

        /////////////////////////////////////////////////////////////////////
        // PROPERTIES
        /////////////////////////////////////////////////////////////////////

        public Node Node
        {
            get { return node; }
        }

        public override Transform3D Transform3D
        {
            get { return transform3D; }
        }

        public override MaterialsMap MaterialsMap
        {
            get { return materialsMap; }
        }

        /////////////////////////////////////////////////////////////////////
        // CONSTRUCTORS
        /////////////////////////////////////////////////////////////////////

        private NodeMarker(Node node)
            : base(node)
        {
            this.node = node;
        }

        public static Entity Create(Node node)
        {
            return new Entity() { IsActive = false, IsVisible = false }
                .AddComponent(new Transform3D())
                .AddComponent(new MaterialsMap(PlaceHolderMaterial))
                .AddComponent(Model.CreatePyramid(10f))
                .AddComponent(new ModelRenderer())
                .AddComponent(new BoxCollider())
                .AddComponent(new NodeMarker(node))
                .AddComponent(new NodeLineBatch())
                .AddChild(new Entity() { IsActive = false, IsVisible = false }
                    .AddComponent(new Transform3D())
                    .AddComponent(new MaterialsMap(SelectedMaterial))
                    .AddComponent(Model.CreateTorus(18, 1, 10))
                    .AddComponent(new ModelRenderer())
                );
        }

        /////////////////////////////////////////////////////////////////////
        // PROTECTED METHODS
        /////////////////////////////////////////////////////////////////////

        protected override void Initialize()
        {
            base.Initialize();
            node.OnNodeLocationChanged += OnNodeLocationChanged;
            node.OnNodeTimedOutChanged += OnNodeTimedOutChanged;
            UpdateMarkerState();
        }

        protected override void Update(TimeSpan gameTime)
        {

        }

        /////////////////////////////////////////////////////////////////////
        // PRIVATE METHODS
        /////////////////////////////////////////////////////////////////////

        private void OnNodeLocationChanged(Node node)
        {
            UpdateMarkerState();
        }

        private void OnNodeTimedOutChanged(Node node)
        {
            UpdateMarkerState();
        }
    }
}
