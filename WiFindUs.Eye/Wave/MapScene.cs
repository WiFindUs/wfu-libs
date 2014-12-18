using System;
using WaveEngine.Common.Graphics;
using WaveEngine.Common.Math;
using WaveEngine.Components.Cameras;
using WaveEngine.Components.Graphics3D;
using WaveEngine.Framework;
using WaveEngine.Framework.Graphics;
using WaveEngine.Framework.Physics3D;
using WaveEngine.Framework.Services;
using WaveEngine.Materials;
using WiFindUs.Controls;
using WiFindUs.Extensions;

namespace WiFindUs.Eye.Wave
{
    public class MapScene : Scene, IThemeable
    {
        public event Action<MapScene> SceneStarted;
        public event Action<MapScene> CameraChanged, CameraFrustumChanged;
        
        private const float CAM_MIN_ZOOM = 100.0f;
        private const float CAM_MAX_ZOOM = 2000.0f;
        private const float CAM_MIN_ANGLE = (float)(Math.PI/7.0);
        private const float CAM_MAX_ANGLE = (float)(Math.PI/2.01);
        private const float ZOOM_RATE = 1.0f;
        private const float TILT_RATE = 1.0f;
        public const uint MIN_LEVEL = Region.GOOGLE_MAPS_TILE_MIN_ZOOM+1;

        private MapGame hostGame;
        private EyeMainForm eyeForm;
        private Theme theme;
        private FixedCamera camera;
        private ILocation center;
        private Entity[][,] tiles;
        private Entity groundPlane;
        private BoxCollider groundPlaneCollider;
        private TerrainTile baseTile;
        private uint visibleLayer = uint.MaxValue;
        private int cameraZoom = 100; //percentage; 100 is all the way zoomed out
        private int cameraTilt = 0; //percentage; 100 is completely vertical
        private Camera3D cameraTransform;
        private bool autoUpdateCamera = true;
        private bool cameraDirty = false;
        private static readonly Color[] colours = new Color[] { Color.Blue, Color.Red, Color.Green };
        private static int lastColor = 0;
        private Ray cameraRay;
        
        //camera frustum
        private ILocation cameraNW, cameraSW, cameraNE, cameraSE, cameraPos, cameraAim;

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
                UpdateCameraPosition();
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

                Tiles((tile) =>
                {
                    tile.Owner.IsVisible
                        = tile.Owner.IsActive
                        = tile.Owner.FindComponent<BoxCollider>().IsActive
                        = false;
                },(int)visibleLayer, (int)visibleLayer);

                visibleLayer = layer;

                Tiles((tile) =>
                {
                    tile.Owner.IsVisible
                        = tile.Owner.IsActive
                        = tile.Owner.FindComponent<BoxCollider>().IsActive
                        = true;
                },(int)visibleLayer, (int)visibleLayer);
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

        public TerrainTile BaseTile
        {
            get { return baseTile; }
        }

        public ILocation CameraNorthWest
        {
            get { return cameraNW; }
        }

        public ILocation CameraNorthEast
        {
            get { return cameraNE; }
        }

        public ILocation CameraSouthEast
        {
            get { return cameraSE; }
        }

        public ILocation CameraSouthWest
        {
            get { return cameraSW; }
        }

        public ILocation CameraLocation
        {
            get { return cameraPos; }
        }

        public ILocation CameraAimLocation
        {
            get { return cameraAim; }
        }

        public MapGame HostGame
        {
            get { return hostGame; }
        }

        public MapApplication HostApplication
        {
            get { return hostGame.HostApplication; }
        }

        public MapControl HostControl
        {
            get { return hostGame.HostControl; }
        }

        public bool DebugMode
        {
            get
            {
                return RenderManager.DebugLines;
            }
            set
            {
                if (value == DebugMode)
                    return;
                RenderManager.DebugLines = value;
                WaveServices.ScreenContextManager.SetDiagnosticsActive(value);
            }
        }

        /////////////////////////////////////////////////////////////////////
        // CONSTRUCTORS
        /////////////////////////////////////////////////////////////////////

        public MapScene(MapGame hostGame)
        {
            if (hostGame == null)
                throw new ArgumentNullException("hostGame", "MapScene cannot be instantiated outside of a host MapGame.");
            this.hostGame = hostGame;
            HostApplication.ScreenResized += HostApplication_ScreenResized;
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

        public ILocation VectorToLocation(Vector3 vec)
        {
            if (baseTile == null || baseTile.Region == null)
                return WiFindUs.Eye.Location.EMPTY;
            
            return new Location(
                baseTile.Region.NorthWest.Latitude - ((vec.Z - (baseTile.Size / -2f)) / baseTile.Size) * baseTile.Region.LatitudinalSpan,
                baseTile.Region.NorthWest.Longitude + ((vec.X - (baseTile.Size / -2f)) / baseTile.Size) * baseTile.Region.LongitudinalSpan
                );
        }

        public ILocation LocationFromScreenRay(int x, int y)
        {
            if (groundPlaneCollider == null)
                return null;
            
            //convert screen to world
            Vector3 screenCoords = new Vector3(x, y, 0.0f);
            Vector3 screenCoordsFar = new Vector3(x, y, 1.0f);
            screenCoords = cameraTransform.Unproject(ref screenCoords);
            screenCoordsFar = cameraTransform.Unproject(ref screenCoordsFar);

            //update ray
            Vector3 rayDirection = screenCoordsFar - screenCoords;
            rayDirection.Normalize();
            cameraRay.Direction = rayDirection;
            cameraRay.Position = screenCoords;

            //test for collision with ground plane
            float? result = groundPlaneCollider.Intersects(ref cameraRay);
            if (!result.HasValue)
                return null;

            //return result
            return VectorToLocation(cameraRay.Position + cameraRay.Direction * result.Value);
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
#if DEBUG
            DebugMode = true;
#endif
            
            //set up camera
            Debugger.V("MapScene: initializing camera");
            cameraRay = new Ray();
            camera = new FixedCamera("camera", Vector3.Up * 200.0f, Vector3.Zero)
            {
                NearPlane = 1f,
                FarPlane = 100000.0f,
                ClearFlags = ClearFlags.Target | ClearFlags.DepthAndStencil,
                BackgroundColor = theme != null ? new Color(
                    theme.ControlDarkColour.R, theme.ControlDarkColour.G,
                    theme.ControlDarkColour.B, theme.ControlDarkColour.A)
                    : Color.CornflowerBlue
            };
            cameraTransform = camera.Entity.FindComponent<Camera3D>();
            EntityManager.Add(camera);
            UpdateCameraPosition();

            //create global lighting
            Debugger.V("MapScene: creating lighting");
            Vector3 sun = new Vector3(0f, 100f, 25f);
            sun.Normalize();
            DirectionalLight skylight = new DirectionalLight("SkyLight", sun);
            EntityManager.Add(skylight);

            //create terrain layers
            Debugger.V("MapScene: creating layers");
            tiles = new Entity[Region.GOOGLE_MAPS_TILE_MAX_ZOOM - MIN_LEVEL + 1][,];
            for (uint layer = 0; layer < tiles.Length; layer++)
                CreateTileLayer(layer);

            //create ground plane
            Debugger.V("MapScene: creating ground plane");
            groundPlane = new Entity()
                .AddComponent(new Transform3D() { Position = new Vector3(0f, 0f, 0f) })
                .AddComponent(Model.CreatePlane(Vector3.UnitY, baseTile.Size * 50f))
                .AddComponent(groundPlaneCollider = new BoxCollider());
            EntityManager.Add(groundPlane);

            //add scene behaviours
            Debugger.V("MapScene: creating behaviours");
            AddSceneBehavior(new MapSceneInputBehaviour(), SceneBehavior.Order.PostUpdate);
        }

        protected override void Start()
        {
            base.Start();
            Tiles((tile) => tile.CalculatePosition());
            VisibleLayer = 0;
            eyeForm = (WFUApplication.MainForm as EyeMainForm);
            foreach (Device device in eyeForm.Devices)
            {
                if (!device.Loaded)
                    continue;
                Device_OnDeviceLoaded(device);
            }
            Device.OnDeviceLoaded += Device_OnDeviceLoaded;
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

            //set position
            float angle = CAM_MIN_ANGLE + ((CAM_MAX_ANGLE - CAM_MIN_ANGLE) * ((float)cameraTilt / 100.0f));
            float distance = CAM_MIN_ZOOM + ((CAM_MAX_ZOOM - CAM_MIN_ZOOM) * ((float)cameraZoom / 100.0f));
            Vector3 direction = new Vector3(0f, (float)Math.Sin(angle), (float)Math.Cos(angle));
            direction.Normalize();
            cameraTransform.Position = new Vector3(
                cameraTransform.LookAt.X,
                cameraTransform.LookAt.Y + (direction.Y * distance),
                cameraTransform.LookAt.Z + (direction.Z * distance));

            //update location 'frustum'
            UpdateCameraFrustum();

            //state
            cameraDirty = false;
            if (CameraChanged != null)
                CameraChanged(this);
        }

        private void UpdateCameraFrustum()
        {
            cameraPos = VectorToLocation(cameraTransform.Position);
            cameraAim = VectorToLocation(cameraTransform.LookAt);
            cameraNW = LocationFromScreenRay(0, 0);
            cameraNE = LocationFromScreenRay(HostApplication.Width, 0);
            cameraSE = LocationFromScreenRay(HostApplication.Width, HostApplication.Height);
            cameraSW = LocationFromScreenRay(0, HostApplication.Height);

            if (CameraFrustumChanged != null)
                CameraFrustumChanged(this);
        }

        private void Device_OnDeviceLoaded(Device device)
        {
            EntityManager.Add(DeviceMarker.Create(device));
        }

        private void CreateTileLayer(uint layer)
        {
            if (tiles == null || layer >= tiles.Length || tiles[layer] != null)
                return;
            
            int depth = 1 << (int)layer;
            tiles[layer] = new Entity[depth, depth];
            for (uint row = 0; row < depth; row++)
            {
                for (uint column = 0; column < depth; column++)
                {
                    Entity tileEntity = tiles[layer][row, column] = TerrainTile.Create(layer, row, column, baseTile);
                    EntityManager.Add(tileEntity);
                    if (layer == 0)
                        baseTile = tileEntity.FindComponent<TerrainTile>();
                    
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

        private void HostApplication_ScreenResized(MapApplication obj)
        {
            UpdateCameraFrustum();
        }
    }
}
