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
using WaveEngine.Materials;
using WiFindUs.Extensions;

namespace WiFindUs.Eye.Wave
{
    public abstract class EntityMarker<T> : Behavior where T : class, ILocatable, ISelectableEntity, IUpdateable
    {
        private const float MAX_SPIN_RATE = 5.0f;
        protected readonly T entity;
        private MapScene scene;
        private Entity selectionRing = null;
        private static Material placeHolderMaterial, selectedMaterial;
        private readonly static Dictionary<string, Material> typeColours = new Dictionary<string, Material>();
        private Transform3D transform3D;
        private MaterialsMap materialsMap;

        /////////////////////////////////////////////////////////////////////
        // PROPERTIES
        /////////////////////////////////////////////////////////////////////

        public T Entity
        {
            get { return entity; }
        }

        public virtual bool VisibleOnTimeout
        {
            get { return false; }
        }

        public static Material PlaceHolderMaterial
        {
            get
            {
                if (placeHolderMaterial == null)
                    placeHolderMaterial = new BasicMaterial(Color.Gray) { LightingEnabled = true };
                return placeHolderMaterial;
            }
        }

        public static Material SelectedMaterial
        {
            get
            {
                if (selectedMaterial == null)
                    selectedMaterial = new BasicMaterial(Color.Yellow) { LightingEnabled = false, Alpha = 0.01f, LayerType = DefaultLayers.Alpha };
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
                else if (age >= Device.TIMEOUT)
                    return 0.0f;
                else
                    return MAX_SPIN_RATE * (1.0f - (entity.UpdateAge / (float)entity.TimeoutLength));
            }
        }

        public MapScene Scene
        {
            get { return scene; }
        }

        public Transform3D Transform3D
        {
            get { return transform3D; }
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
                    = new BasicMaterial(new Color(col.R, col.G, col.B, col.A)) { LightingEnabled = true };
            }
            return material;     
        }

        /////////////////////////////////////////////////////////////////////
        // PROTECTED METHODS
        /////////////////////////////////////////////////////////////////////

        protected override void Initialize()
        {
            base.Initialize();
            transform3D = Owner.FindComponent<Transform3D>();
            materialsMap = Owner.FindComponent<MaterialsMap>();
            scene = Owner.Scene as MapScene;
            selectionRing = Owner.ChildEntities.First<Entity>();
            entity.SelectedChanged += SelectedChanged;
            entity.LocationChanged += LocationChanged;
            entity.TimedOutChanged += TimedOutChanged;
            scene.BaseTile.CenterLocationChanged += BaseTileCenterLocationChanged;
        }

        protected override void Update(TimeSpan gameTime)
        {
            if (entity == null || transform3D == null || !Owner.IsVisible)
                return;
            float rot = RotationSpeed;
            if (!rot.Tolerance(0.0f, 0.0001f))
            {
                transform3D.Rotation = new Vector3(
                    transform3D.Rotation.X,
                    transform3D.Rotation.Y + rot * (float)gameTime.TotalSeconds,
                    transform3D.Rotation.Z);
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

        protected virtual void SelectedChanged(ISelectableEntity obj)
        {
            UpdateMarkerState();
        }

        protected virtual void TimedOutChanged(IUpdateable obj)
        {
            UpdateMarkerState();
        }

        protected virtual void UpdateMarkerState()
        {
            if (Scene == null)
                return;

            bool active = Scene != null
                && Scene.BaseTile != null
                && Scene.BaseTile.Region != null
                && (VisibleOnTimeout || !entity.TimedOut)
                && entity.Location.HasLatLong
                && Scene.BaseTile.Region.Contains(entity.Location);

            Owner.IsActive = Owner.IsVisible = active;
            selectionRing.IsActive = selectionRing.IsVisible = active && entity.Selected;
            if (active)
            {
                Vector3 pos = scene.LocationToVector(entity.Location);
                pos.Y += 5.0f;
                transform3D.Position = pos;
                materialsMap.DefaultMaterial = CurrentMaterial;
            }
        }
    }
}
