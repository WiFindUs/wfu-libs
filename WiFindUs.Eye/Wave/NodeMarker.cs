﻿using System;
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
                //base
                .AddComponent(new Transform3D())
                .AddComponent(new NodeMarker(node))
                .AddComponent(new NodeLineBatch())
                //model
                .AddChild(new Entity("model") { IsActive = false, IsVisible = false }
                    .AddComponent(new Transform3D() { Position = new Vector3(0.0f, 5.0f, 0.0f) })
                    .AddComponent(new MaterialsMap(PlaceHolderMaterial))
                    .AddComponent(Model.CreateCube(10f))
                    .AddComponent(new ModelRenderer())
                    .AddComponent(new BoxCollider() { IsActive = false }))
                //selection ring
                .AddChild(new Entity("selection") { IsActive = false, IsVisible = false }
                    .AddComponent(new Transform3D() { Position = new Vector3(0.0f, 5.0f, 0.0f) })
                    .AddComponent(new MaterialsMap(SelectedMaterial))
                    .AddComponent(Model.CreateTorus(18, 1, 12))
                    .AddComponent(new ModelRenderer()));
        }
    }
}
