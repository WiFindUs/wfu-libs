using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WaveEngine.Common.Graphics;
using WaveEngine.Common.Math;
using WaveEngine.Components.Cameras;
using WaveEngine.Components.Graphics3D;
using WaveEngine.Framework;
using WaveEngine.Framework.Graphics;
using WaveEngine.Framework.Services;
using WaveEngine.Materials;
using WiFindUs.Controls;
using WiFindUs.Extensions;

namespace WiFindUs.Eye.Wave
{
    public class MapScene : Scene, IThemeable
    {
        public static event Action<MapScene> SceneStarted;
        
        private const float CAM_MIN_ZOOM = 100.0f;
        private const float CAM_MAX_ZOOM = 2000.0f;
        private const float CAM_MIN_ANGLE = (float)(Math.PI/8.0);
        private const float CAM_MAX_ANGLE = (float)(Math.PI/2.01);
        private const float ZOOM_RATE = 1.0f;
        private const float TILT_RATE = 1.0f;
        public const uint MIN_LEVEL = Region.GOOGLE_MAPS_TILE_MIN_ZOOM+1;

        private EyeMainForm eyeForm;
        private Theme theme;
        private FixedCamera camera;
        private ILocation center;
        private Entity[][,] tiles;
        private TerrainTile baseTile;
        private uint visibleLayer = 0;
        private int cameraZoom = 100; //percentage; 100 is all the way zoomed out
        private int cameraTilt = 50; //percentage; 100 is completely vertical
        private Camera3D cameraTransform;
        private bool autoUpdateCamera = true;
        private bool cameraDirty = false;
        private static readonly Color[] colours = new Color[] { Color.Blue, Color.Red, Color.Green };
        private static int lastColor = 0;

        /////////////////////////////////////////////////////////////////////
        // PROPERTIES
        /////////////////////////////////////////////////////////////////////

        public Theme Theme
        {
            get
            {
                return theme;
            }
            set
            {
                if (value == null || value == theme)
                    return;

                theme = value;
                if (camera != null)
                    camera.BackgroundColor = new Color(
                        theme.ControlDarkColour.R, theme.ControlDarkColour.G,
                        theme.ControlDarkColour.B, theme.ControlDarkColour.A);
            }
        }

        public ILocation CenterLocation
        {
            get { return center; }
            set
            {
                if (value == null || value.Equals(center))
                    return;
                Debugger.I("Setting map center to " + value.ToString());
                center = value;
                UpdateTileLocations();
            }
        }

        public bool CameraAutoUpdate
        {
            get { return autoUpdateCamera; }
            set
            {
                autoUpdateCamera = value;
                if (autoUpdateCamera && cameraDirty)
                    UpdateCameraPosition();
            }
        }

        public uint VisibleLayer
        {
            get
            {
                return visibleLayer;
            }
            protected set
            {
                uint layer = value >= tiles.Length ? (uint)tiles.Length - 1 : value;
                if (layer == visibleLayer)
                    return;

                Tiles((tile) => { tile.Owner.IsVisible = tile.Owner.IsActive = false; },
                    (int)visibleLayer, (int)visibleLayer);
                visibleLayer = layer;
                Tiles((tile) => { tile.Owner.IsVisible = tile.Owner.IsActive = true; },
                    (int)visibleLayer, (int)visibleLayer);
            }
        }

        public uint LayerCount
        {
            get { return (uint)tiles.Length; }
        }

        public int CameraZoom
        {
            get { return cameraZoom; }
            set
            {
                int zoom = value < 0 ? 0 : (value > 100 ? 100 : value);
                if (zoom == cameraZoom)
                    return;
                cameraZoom = zoom;
                VisibleLayer = (uint)((1.0f - ((float)cameraZoom / 100.0f)) * (float)tiles.Length);
                if (autoUpdateCamera)
                    UpdateCameraPosition();
                else
                    cameraDirty = true;
            }
        }

        public int CameraTilt
        {
            get { return cameraTilt; }
            set
            {
                int tilt = value < 0 ? 0 : (value > 100 ? 100 : value);
                if (tilt == cameraTilt)
                    return;
                cameraTilt = tilt;
                if (autoUpdateCamera)
                    UpdateCameraPosition();
                else
                    cameraDirty = true;
            }
        }

        public Vector3 CameraTarget
        {
            get { return cameraTransform == null ? Vector3.Zero : cameraTransform.LookAt; }
            set
            {
                if (cameraTransform == null)
                    return;
                cameraTransform.LookAt = new Vector3(
                    value.X < baseTile.TopLeft.X ? baseTile.TopLeft.X : (value.X > baseTile.BottomRight.X ? baseTile.BottomRight.X : value.X),
                    value.Y,
                    value.Z < baseTile.TopLeft.Z ? baseTile.TopLeft.Z : (value.Z > baseTile.BottomRight.Z ? baseTile.BottomRight.Z : value.Z)
                    );
                if (autoUpdateCamera)
                    UpdateCameraPosition();
                else
                    cameraDirty = true;
            }
        }

        /////////////////////////////////////////////////////////////////////
        // PUBLIC METHODS
        /////////////////////////////////////////////////////////////////////
        
        public Vector3 LocationToVector(ILocation loc)
        {
            if (baseTile == null)
                return Vector3.Zero;
            return baseTile.LocationToVector(loc);
        }

        public void CancelThreads()
        {
            Tiles((tile) => tile.CancelThreads());
        }

        /////////////////////////////////////////////////////////////////////
        // PROTECTED METHODS
        /////////////////////////////////////////////////////////////////////
        
        protected override void CreateScene()
        {
            //set up camera
            Debugger.V("MapScene: initializing camera");
            camera = new FixedCamera("camera", Vector3.Up*200.0f,Vector3.Zero);
            camera.NearPlane = 1f;
            camera.FarPlane = 100000.0f;
            if (theme != null)
            {
                camera.BackgroundColor = new Color(
                    theme.ControlDarkColour.R, theme.ControlDarkColour.G,
                    theme.ControlDarkColour.B, theme.ControlDarkColour.A);
            }
            else
                camera.BackgroundColor = Color.CornflowerBlue;
            camera.ClearFlags = ClearFlags.Target | ClearFlags.DepthAndStencil;
            cameraTransform = camera.Entity.FindComponent<Camera3D>();
            EntityManager.Add(camera);
            UpdateCameraPosition();

            //create global lighting
            Vector3 sun = new Vector3(0f, 100f, 25f);
            sun.Normalize();
            DirectionalLight skylight = new DirectionalLight("SkyLight", sun);
            EntityManager.Add(skylight);

            //create terrain layers
            Debugger.V("MapScene: creating layers");
            tiles = new Entity[Region.GOOGLE_MAPS_TILE_MAX_ZOOM - MIN_LEVEL + 1][,];
            for (uint layer = 0; layer < tiles.Length; layer++)
                CreateTileLayer(layer);

            //add scene behaviours
            Debugger.V("MapScene: creating behaviours");
            AddSceneBehavior(new MapSceneInputBehaviour(), SceneBehavior.Order.PostUpdate);
            AddSceneBehavior(new CameraMovementBehavior(), SceneBehavior.Order.PostUpdate);
        }

        protected override void Start()
        {
            base.Start();
            Tiles((tile) => tile.CalculatePosition());
            tiles[0][0, 0].IsVisible = true;
            eyeForm = (WFUApplication.MainForm as EyeMainForm);
            foreach (Device device in eyeForm.Devices)
                Device_OnDeviceCreated(device);
            Device.OnDeviceCreated += Device_OnDeviceCreated;
            if (SceneStarted != null)
                SceneStarted(this);
        }

        /////////////////////////////////////////////////////////////////////
        // PRIVATE METHODS
        /////////////////////////////////////////////////////////////////////

        private void UpdateCameraPosition()
        {
            if (cameraTransform == null)
                return;

            float angle = CAM_MIN_ANGLE + ((CAM_MAX_ANGLE - CAM_MIN_ANGLE) * ((float)cameraTilt / 100.0f));
            float distance = CAM_MIN_ZOOM + ((CAM_MAX_ZOOM - CAM_MIN_ZOOM) * ((float)cameraZoom / 100.0f));
            Vector3 direction = new Vector3(0f, (float)Math.Sin(angle), (float)Math.Cos(angle));
            direction.Normalize();
            cameraTransform.Position = new Vector3(
                cameraTransform.LookAt.X,
                cameraTransform.LookAt.Y + (direction.Y * distance),
                cameraTransform.LookAt.Z + (direction.Z * distance));

            cameraDirty = false;
        }

        private void Device_OnDeviceCreated(Device sender)
        {
            Entity device = new Entity()
                .AddComponent(new Transform3D() { Rotation = new Vector3(180.0f.ToRadians(), 0f, 0f) })
                .AddComponent(new MaterialsMap(new BasicMaterial(colours[lastColor = (lastColor + 1) % colours.Length])
                { LightingEnabled = true }))
                .AddComponent(Model.CreateCone(10f, 6f, 6))
                .AddComponent(new ModelRenderer())
                .AddComponent(new DeviceBehaviour(sender));
            sender.OnDeviceTypeChanged += sender_OnDeviceTypeChanged;
            EntityManager.Add(device);
        }

        private void sender_OnDeviceTypeChanged(Device obj)
        {
            throw new NotImplementedException();
        }

        private void CreateTileLayer(uint layer)
        {
            if (tiles == null || layer >= tiles.Length || tiles[layer] != null)
                return;
            
            float planeSize = (float)Math.Pow(2.0,
                8.0 + (MIN_LEVEL - Region.GOOGLE_MAPS_TILE_MIN_ZOOM)//smallest chunks will be sized at this power of two
                + (Region.GOOGLE_MAPS_TILE_MAX_ZOOM - Region.GOOGLE_MAPS_TILE_MIN_ZOOM)
                - layer) / 10.0f;

            int depth = 1 << (int)layer;
            tiles[layer] = new Entity[depth, depth];
            for (uint row = 0; row < depth; row++)
            {
                for (uint column = 0; column < depth; column++)
                {
                    TerrainTile tile = new TerrainTile(
                        layer == 0 ? null : baseTile,
                        MIN_LEVEL + layer,
                        row, column,
                        planeSize);
                    if (layer == 0)
                        baseTile = tile;

                    Entity chunkEntity = tiles[layer][row, column] = new Entity()
                    .AddComponent(new Transform3D())
                    .AddComponent(new MaterialsMap((row+column) % 2 == 0 ? TerrainTile.PlaceHolderMaterial : TerrainTile.PlaceHolderMaterialAlt))
                    .AddComponent(Model.CreatePlane(Vector3.UnitY, planeSize))
                    .AddComponent(new ModelRenderer())
                    .AddComponent(tile);
                    EntityManager.Add(chunkEntity);

                    chunkEntity.IsVisible = false;
                }
            }
        }

        private void Tiles(Action<TerrainTile> action, int firstLayer = -1, int lastLayer = -1, int excludeLayer = -1)
        {
            if (action == null || tiles == null || firstLayer >= tiles.Length)
                return;

            firstLayer = firstLayer < 0 ? 0 : firstLayer;
            lastLayer = lastLayer < 0 ? tiles.Length - 1 : (lastLayer < firstLayer ? firstLayer
                : (lastLayer >= tiles.Length ? tiles.Length - 1 : lastLayer));

            for (int layer = firstLayer; layer <= lastLayer; layer++)
            {
                if (layer == excludeLayer)
                    continue;
                int depth = 1 << (int)layer;
                for (uint row = 0; row < depth; row++)
                    for (uint column = 0; column < depth; column++)
                        action(tiles[layer][row, column].FindComponent<TerrainTile>());
            }
        }

        private void UpdateTileLocations()
        {
            if (center == null || tiles == null || baseTile == null)
                return;

            baseTile.CenterLocation = center;
            Tiles((tile) =>
            {
                float ratio = (tile.Size / baseTile.Size);
                float latSize = (float)baseTile.Region.LatitudinalSpan * ratio;
                float longSize = (float)baseTile.Region.LongitudinalSpan * ratio;

                tile.CenterLocation = new Location(
                    baseTile.Region.NorthWest.Latitude.Value - latSize * ((float)tile.Row + 0.5f), //lat
                    baseTile.Region.NorthWest.Longitude.Value + longSize * ((float)tile.Column + 0.5f)//long
                    );
            }, 1);
        }

        private class CameraMovementBehavior : SceneBehavior
        {
            protected override void ResolveDependencies()
            {

            }

            protected override void Update(TimeSpan gameTime)
            {


            }
        };
    }
}
