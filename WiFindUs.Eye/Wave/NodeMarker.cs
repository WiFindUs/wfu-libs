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
    public class NodeMarker : EntityMarker<Node>
    {
        /////////////////////////////////////////////////////////////////////
        // CONSTRUCTORS
        /////////////////////////////////////////////////////////////////////

        public NodeMarker(Node n)
            : base(n)
        {

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
    }
}
