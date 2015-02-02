﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WaveEngine.Common.Graphics;
using WaveEngine.Common.Math;
using WaveEngine.Components.Graphics3D;
using WaveEngine.Components.UI;
using WaveEngine.Framework;
using WaveEngine.Framework.Graphics;
using WaveEngine.Framework.Physics3D;
using WaveEngine.Materials;
using WiFindUs.Extensions;

namespace WiFindUs.Eye.Wave
{
    public class DeviceMarker : EntityMarker<Device>
    {
        
        /////////////////////////////////////////////////////////////////////
        // PROPERTIES
        /////////////////////////////////////////////////////////////////////

        public override Material CurrentMaterial
        {
            get
            {
                return entity.User == null ? base.CurrentMaterial : TypeMaterial(entity.User.Type);
            }
        }

        /////////////////////////////////////////////////////////////////////
        // CONSTRUCTORS
        /////////////////////////////////////////////////////////////////////

        public DeviceMarker(Device d)
            : base(d)
        {

        }

        public static Entity Create(Device device)
        {
            return new Entity() { IsActive = false, IsVisible = false }
                //base
                .AddComponent(new Transform3D())
                .AddComponent(new DeviceMarker(device))
                //model
                .AddChild(new Entity("model") { IsActive = false, IsVisible = false }
                    .AddComponent(new Transform3D()
                    {
                        Position = new Vector3(0.0f, 5.0f, 0.0f),
                        Rotation = new Vector3(180.0f.ToRadians(), 0f, 0f)
                    })
                    .AddComponent(new MaterialsMap(PlaceHolderMaterial))
                    .AddComponent(Model.CreateCone(10f, 6f, 5))
                    .AddComponent(new ModelRenderer())
                    .AddComponent(new BoxCollider() { IsActive = false, DebugLineColor = Color.Gray }))
                //selection ring
                .AddChild(new Entity("selection") { IsActive = false, IsVisible = false }
                    .AddComponent(new Transform3D()
                    {
                        Position = new Vector3(0.0f, 5.0f, 0.0f)
                    })
                    .AddComponent(new MaterialsMap(SelectedMaterial))
                    .AddComponent(Model.CreateTorus(13, 1, 12))
                    .AddComponent(new ModelRenderer()));
        }

        /////////////////////////////////////////////////////////////////////
        // PROTECTED METHODS
        /////////////////////////////////////////////////////////////////////

        protected override void Initialize()
        {
            base.Initialize();
            entity.OnDeviceUserChanged += OnDeviceUserChanged;
            UpdateMarkerState();
        }

        /////////////////////////////////////////////////////////////////////
        // PRIVATE METHODS
        /////////////////////////////////////////////////////////////////////

        private void OnDeviceUserChanged(Device device)
        {
            UpdateMarkerState();
            if (BoxCollider != null)
                BoxCollider.DebugLineColor = entity.User == null ? Color.Gray : TypeColor(entity.User.Type);
        }
    }
}
