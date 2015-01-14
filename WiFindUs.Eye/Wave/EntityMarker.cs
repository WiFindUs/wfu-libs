using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WaveEngine.Common.Graphics;
using WaveEngine.Common.Math;
using WaveEngine.Components.Graphics3D;
using WaveEngine.Framework;
using WaveEngine.Framework.Graphics;
using WaveEngine.Framework.Physics3D;
using WaveEngine.Materials;
using WiFindUs.Extensions;

namespace WiFindUs.Eye.Wave
{
    public class EntityMarker<T> : Marker where T : class, ILocatable, ISelectable, IUpdateable
    {
        protected readonly T entity;

        private Entity selectionRing = null, model = null;
        private static Material placeHolderMaterial, selectedMaterial;
        private readonly static Dictionary<string, Material> typeColours = new Dictionary<string, Material>();
        private Transform3D modelTransform, selectionRingTransform;
        private MaterialsMap modelMaterialsMap;
        private BoxCollider modelCollider;

        /////////////////////////////////////////////////////////////////////
        // PROPERTIES
        /////////////////////////////////////////////////////////////////////

        public T Entity
        {
            get { return entity; }
        }

        public virtual bool VisibleOnTimeout
        {
            get
            {
#if DEBUG
                return true;
#else
                return false;
#endif
            }
        }

        public static Material PlaceHolderMaterial
        {
            get
            {
                if (placeHolderMaterial == null)
                    placeHolderMaterial = new BasicMaterial(Color.Gray)
                    {
                        LightingEnabled = true,
                        AmbientLightColor = Color.White * 0.5f,
                        SpecularPower = 2
                    };
                    
                return placeHolderMaterial;
            }
        }

        public static Material SelectedMaterial
        {
            get
            {
                if (selectedMaterial == null)
                    selectedMaterial = new BasicMaterial(Color.Yellow)
                    {
                        LayerType = DefaultLayers.Alpha,
                        Alpha = 0.05f
                    };
                return selectedMaterial;
            }
        }

        public virtual Material CurrentMaterial
        {
            get { return PlaceHolderMaterial; }
        }

        public virtual float RotationSpeed
        {
            get
            {
                if (entity.TimedOut)
                    return 0.0f;

                long age = entity.UpdateAge;
                if (age == 0)
                    return MAX_SPIN_RATE;
                else if (age >= entity.TimeoutLength)
                    return 0.0f;
                else
                    return MAX_SPIN_RATE * (1.0f - (entity.UpdateAge / (float)entity.TimeoutLength));
            }
        }

        public override BoxCollider BoxCollider
        {
            get { return modelCollider; }
        }

        public override bool Selected
        {
            get { return entity.Selected; }
            set { entity.Selected = value; }
        }

        /////////////////////////////////////////////////////////////////////
        // CONSTRUCTORS
        /////////////////////////////////////////////////////////////////////

        public EntityMarker(T entity)
        {
            if (entity == null)
                throw new ArgumentNullException("entity", "Entity cannot be null!");
            this.entity = entity;
        }

        /////////////////////////////////////////////////////////////////////
        // PUBLIC METHODS
        /////////////////////////////////////////////////////////////////////

        public static Material TypeMaterial(String type)
        {
            Material material = PlaceHolderMaterial;
            if (type == null || (type = type.Trim().ToLower()).Length == 0)
                return material;

            if (!typeColours.TryGetValue(type, out material))
            {
                System.Drawing.Color col
                    = WFUApplication.Config.Get("type_" + type + ".colour", System.Drawing.Color.Gray);
                typeColours[type] = material
                    = new BasicMaterial(new Color(col.R, col.G, col.B, col.A))
                    {
                        LightingEnabled = true,
                        AmbientLightColor = Color.White * 0.5f,
                        SpecularPower = 2
                    };
            }
            return material;
        }

        /////////////////////////////////////////////////////////////////////
        // PROTECTED METHODS
        /////////////////////////////////////////////////////////////////////

        protected override void Initialize()
        {
            base.Initialize();
            model = Owner.FindChild("model");
            if (model != null)
            {
                modelMaterialsMap = model.FindComponent<MaterialsMap>();
                modelTransform = model.FindComponent<Transform3D>();
                modelCollider = model.FindComponent<BoxCollider>();
            }
            selectionRing = Owner.FindChild("selection");
            if (selectionRing != null)
                selectionRingTransform = selectionRing.FindComponent<Transform3D>();

            entity.SelectedChanged += SelectedChanged;
            entity.LocationChanged += LocationChanged;
            entity.TimedOutChanged += TimedOutChanged;
            Scene.BaseTile.CenterLocationChanged += BaseTileCenterLocationChanged;

            UpdateMarkerState();
        }

        protected override void Update(TimeSpan gameTime)
        {
            if (entity == null || !Owner.IsVisible)
                return;

            if (model != null && modelTransform != null)
            {
                float rot = RotationSpeed;
                if (!rot.Tolerance(0.0f, 0.0001f))
                {
                    modelTransform.Rotation = new Vector3(
                        modelTransform.Rotation.X,
                        modelTransform.Rotation.Y + rot * (float)gameTime.TotalSeconds,
                        modelTransform.Rotation.Z);
                }
            }

            if (entity.Selected && selectionRing != null && selectionRingTransform != null)
            {
                selectionRingTransform.Rotation = new Vector3(
                    selectionRingTransform.Rotation.X,
                    selectionRingTransform.Rotation.Y + 1.0f * (float)gameTime.TotalSeconds,
                    selectionRingTransform.Rotation.Z);
            }
        }

        protected virtual void BaseTileCenterLocationChanged(TerrainTile obj)
        {
            UpdateMarkerState();
        }

        protected virtual void LocationChanged(ILocatable obj)
        {
            UpdateMarkerState();
        }

        protected virtual void SelectedChanged(ISelectable obj)
        {
            UpdateMarkerState();
        }

        protected virtual void TimedOutChanged(IUpdateable obj)
        {
            UpdateMarkerState();
        }

        protected virtual void UpdateMarkerState()
        {
            bool active = Scene.BaseTile != null
                && Scene.BaseTile.Region != null
                && (VisibleOnTimeout || !entity.TimedOut)
                && entity.Location.HasLatLong
                && Scene.BaseTile.Region.Contains(entity.Location);

            Owner.IsActive = Owner.IsVisible = active;
            if (model != null)
            {
                model.IsActive = model.IsVisible = active;
                if (modelCollider != null)
                    modelCollider.IsActive = active;
            }
            if (selectionRing != null)
                selectionRing.IsActive = selectionRing.IsVisible = active && entity.Selected;
            if (active)
            {
                if (Transform3D != null)
                    Transform3D.Position = Scene.LocationToVector(entity.Location);
                if (modelMaterialsMap != null)
                    modelMaterialsMap.DefaultMaterial = CurrentMaterial;
            }
        }
    }
}
