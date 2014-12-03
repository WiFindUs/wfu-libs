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

namespace WiFindUs.Eye.Wave
{
    public class MapScene : Scene, IThemeable
    {
        public static event Action<MapScene> SceneStarted;
        
        private const float CAM_MIN_ZOOM = 100.0f;
        private const float CAM_MAX_ZOOM = 10000.0f;
        private const float CAM_MIN_ANGLE = (float)(Math.PI/8.0);
        private const float CAM_MAX_ANGLE = (float)(Math.PI/2.1);

        private EyeContext eyeContext;
        private Theme theme;
        private FixedCamera camera;
        private ILocation center;
        private Entity[][,] chunks;
        private TerrainChunk baseChunk;
        private uint visibleLayer = 0;
        private int cameraZoom = 50; //percentage
        private int cameraTilt = 50; //percentage
        private Camera3D cameraTransform;
        private bool autoUpdateCamera = true;
        private bool cameraDirty = false;

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
                center = value;
                UpdateChunkLocations();
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
            set
            {
                uint layer = value >= chunks.Length ? (uint)chunks.Length - 1 : value;
                if (layer == visibleLayer)
                    return;

                Chunks((chunk) => { chunk.Owner.IsVisible = chunk.Owner.IsActive = false; },
                    (int)visibleLayer, (int)visibleLayer);
                visibleLayer = layer;
                Chunks((chunk) => {chunk.Owner.IsVisible = chunk.Owner.IsActive = true;},
                    (int)visibleLayer, (int)visibleLayer);
            }
        }

        public uint LayerCount
        {
            get { return (uint)chunks.Length; }
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
                cameraTransform.LookAt = value;
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
            return baseChunk.Region.LocationToVector(baseChunk.TopLeft, baseChunk.BottomRight, loc);
        }

        /////////////////////////////////////////////////////////////////////
        // PROTECTED METHODS
        /////////////////////////////////////////////////////////////////////
        
        protected override void CreateScene()
        {
            //set up camera
            camera = new FixedCamera("camera", Vector3.Up*200.0f,Vector3.Zero);
            camera.NearPlane = 0.1f;
            camera.FarPlane = 100000.0f;
            //camera.Entity.AddComponent(new CameraFrustum());
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
            //RenderManager.SetFrustumCullingCamera(camera.Entity);
            EntityManager.Add(camera);
            UpdateCameraPosition();

            //create terrain layers
            chunks = new Entity[Region.GOOGLE_MAPS_CHUNK_MAX_ZOOM - Region.GOOGLE_MAPS_CHUNK_MIN_ZOOM + 1][,];
            for (uint layer = 0; layer < (Region.GOOGLE_MAPS_CHUNK_MAX_ZOOM - Region.GOOGLE_MAPS_CHUNK_MIN_ZOOM + 1); layer++)
                CreateChunkLayer(layer);

            //add scene behaviour
            this.AddSceneBehavior(new MapSceneInputBehaviour(), SceneBehavior.Order.PreUpdate);
        }

        protected override void Start()
        {
            base.Start();
            Chunks((chunk) => chunk.CalculatePosition());
            eyeContext = WFUApplication.MySQLDataContext as WiFindUs.Eye.EyeContext;
            if (SceneStarted != null)
                SceneStarted(this);
            foreach (Device device in eyeContext.Devices)
                Device_OnDeviceCreated(device);
            Device.OnDeviceCreated += Device_OnDeviceCreated;
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
                .AddComponent(new Transform3D())
                .AddComponent(new MaterialsMap(new BasicMaterial(Color.Blue)))
                .AddComponent(Model.CreateCylinder(20.0f, 5.0f, 6))
                .AddComponent(new ModelRenderer())
                .AddComponent(new DeviceBehaviour(sender));
            EntityManager.Add(device);
        }

        public void CreateChunkLayer(uint layer)
        {
            if (chunks == null || layer >= chunks.Length || chunks[layer] != null)
                return;
            
            float planeSize = (float)Math.Pow(2.0,
                9.0 //smallest chunks will be sized at this power of two
                + (Region.GOOGLE_MAPS_CHUNK_MAX_ZOOM - Region.GOOGLE_MAPS_CHUNK_MIN_ZOOM)
                - layer) / 10.0f;

            int depth = 1 << (int)layer;
            chunks[layer] = new Entity[depth, depth];
            for (uint row = 0; row < depth; row++)
            {
                for (uint column = 0; column < depth; column++)
                {
                    TerrainChunk chunk = new TerrainChunk(
                        layer == 0 ? null : baseChunk,
                        Region.GOOGLE_MAPS_CHUNK_MIN_ZOOM + layer,
                        row, column,
                        planeSize);
                    if (layer == 0)
                        baseChunk = chunk;

                    Entity chunkEntity = chunks[layer][row, column] = new Entity()
                    .AddComponent(new Transform3D())
                    .AddComponent(new MaterialsMap())
                    .AddComponent(Model.CreatePlane(Vector3.UnitY, planeSize))
                    .AddComponent(new ModelRenderer())
                    .AddComponent(chunk);
                    EntityManager.Add(chunkEntity);

                    chunkEntity.IsVisible = layer == visibleLayer;
                }
            }
        }

        private void Chunks(Action<TerrainChunk> action, int firstLayer = -1, int lastLayer = -1, int excludeLayer = -1)
        {
            if (action == null || chunks == null || firstLayer >= chunks.Length)
                return;

            firstLayer = firstLayer < 0 ? 0 : firstLayer;
            lastLayer = lastLayer < 0 ? chunks.Length - 1 : (lastLayer < firstLayer ? firstLayer
                : (lastLayer >= chunks.Length ? chunks.Length - 1 : lastLayer));

            for (int layer = firstLayer; layer <= lastLayer; layer++)
            {
                if (layer == excludeLayer)
                    continue;
                int depth = 1 << (int)layer;
                for (uint row = 0; row < depth; row++)
                    for (uint column = 0; column < depth; column++)
                        action(chunks[layer][row, column].FindComponent<TerrainChunk>());
            }
        }

        private void UpdateChunkLocations()
        {
            if (center == null || chunks == null || baseChunk == null)
                return;

            baseChunk.CenterLocation = center;
            Chunks((chunk) =>
            {
                float ratio = (chunk.Size / baseChunk.Size);
                float latSize = (float)baseChunk.Region.LatitudinalSpan * ratio;
                float longSize = (float)baseChunk.Region.LongitudinalSpan * ratio;

                chunk.CenterLocation = new Location(
                    baseChunk.Region.NorthWest.Latitude - latSize * ((float)chunk.Row + 0.5f), //lat
                    baseChunk.Region.NorthWest.Longitude - longSize * ((float)chunk.Column + 0.5f)//long
                    );

            }, 1);
        }
    }
}
