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

namespace WiFindUs.Eye.Wave
{
    public abstract class EntityMarker : Behavior
    {
        private ISelectableEntity entity;
        private ILocatable locatable;
        private MapScene scene;
        private Entity selectionRing = null;
        private static Material placeHolderMaterial, selectedMaterial;
        private readonly static Dictionary<string, Material> UserTypeColours = new Dictionary<string, Material>();

        /////////////////////////////////////////////////////////////////////
        // PROPERTIES
        /////////////////////////////////////////////////////////////////////

        public ISelectableEntity Entity
        {
            get { return entity; }
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

        public MapScene Scene
        {
            get { return scene; }
        }

        public abstract Transform3D Transform3D { get; }

        public abstract MaterialsMap MaterialsMap { get; }

        /////////////////////////////////////////////////////////////////////
        // CONSTRUCTORS
        /////////////////////////////////////////////////////////////////////

        public EntityMarker(ISelectableEntity entity)
        {
            if (entity == null)
                throw new ArgumentNullException("entity", "Entity cannot be null!");
            if ((locatable = (entity as ILocatable)) == null)
                throw new ArgumentOutOfRangeException("entity", "Entity must implement ILocatable!");
            this.entity = entity;
        }

        /////////////////////////////////////////////////////////////////////
        // PUBLIC METHODS
        /////////////////////////////////////////////////////////////////////

        public static Material UserTypeMaterial(User user)
        {
            Material material = PlaceHolderMaterial;
            if (user == null)
                return material;
            string type = user.Type;
            if (type == null || (type = type.Trim().ToLower()).Length == 0)
                return material;

            if (!UserTypeColours.TryGetValue(type, out material))
            {
                System.Drawing.Color col
                    = WFUApplication.Config.Get("type_" + type + ".colour", System.Drawing.Color.Gray);
                UserTypeColours[type] = material
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
            scene = Owner.Scene as MapScene;
            selectionRing = Owner.ChildEntities.First<Entity>();
            entity.SelectedChanged += SelectedChanged;
            scene.BaseTile.CenterLocationChanged += BaseTileCenterLocationChanged;
        }

        protected virtual void BaseTileCenterLocationChanged(TerrainTile obj)
        {
            UpdateMarkerState();
        }

        protected virtual void SelectedChanged(ISelectableEntity obj)
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
                && locatable.Location.HasLatLong
                && Scene.BaseTile.Region.Contains(locatable.Location);

            Owner.IsActive = Owner.IsVisible = active;
            selectionRing.IsActive = selectionRing.IsVisible = active && entity.Selected;
            if (active)
            {
                Vector3 pos = scene.LocationToVector(locatable.Location);
                pos.Y += 5.0f;
                Transform3D.Position = pos;
                MaterialsMap.DefaultMaterial = CurrentMaterial;
            }
        }
    }
}
