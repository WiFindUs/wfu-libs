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
using WiFindUs.Controls;

namespace WiFindUs.Eye.Wave
{
    public class MapScene : Scene, IThemeable
    {        
        private Theme theme;
        private FixedCamera camera;
        private ILocation center;
        private Entity[][,] chunks;
        private TerrainChunk baseChunk;

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
            }
        }

        /////////////////////////////////////////////////////////////////////
        // PROTECTED METHODS
        /////////////////////////////////////////////////////////////////////
        
        protected virtual void OnMapCenterChanged()
        {

        }

        protected override void CreateScene()
        {
            //set up camera
            camera = new FreeCamera("camera", new Vector3(0f, 20000f, -100f), Vector3.Zero);
            camera.NearPlane = 0.1f;
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
            EntityManager.Add(camera);

            //create terrain chunks
            float planeSize = (float)Math.Pow(2.0,
                    9.0 //smallest chunks will be sized at this power of two
                    + (Region.GOOGLE_MAPS_CHUNK_MAX_ZOOM - Region.GOOGLE_MAPS_CHUNK_MIN_ZOOM));
            chunks = new Entity[Region.GOOGLE_MAPS_CHUNK_MAX_ZOOM - Region.GOOGLE_MAPS_CHUNK_MIN_ZOOM + 1][,];
            for (uint layer = 0; layer < chunks.Length; layer++)
            {
                chunks[layer] = new Entity[layer + 1,layer + 1];
                for (uint row = 0; row < (layer + 1); row++)
                {
                    for (uint column = 0; column < (layer + 1); column++)
                    {
                        ModelRenderer modelRenderer = new ModelRenderer();
                        modelRenderer.IsVisible = false;

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
                        .AddComponent(modelRenderer)
                        .AddComponent(chunk);
                        EntityManager.Add(chunkEntity);
                    }
                }
                planeSize /= 2.0f;
            }           
        }

        protected override void Start()
        {
            base.Start();
            Chunks((chunk) => { chunk.SetPosition(); });
        }

        /////////////////////////////////////////////////////////////////////
        // PRIVATE METHODS
        /////////////////////////////////////////////////////////////////////

        private void Chunks(Action<TerrainChunk> action, int firstLayer = -1, int lastLayer = -1)
        {
            if (action == null || chunks == null || firstLayer >= chunks.Length)
                return;

            firstLayer = firstLayer < 0 ? 0 : firstLayer;
            lastLayer = lastLayer < 0 ? chunks.Length - 1 : (lastLayer < firstLayer ? firstLayer
                : (lastLayer >= chunks.Length ? chunks.Length - 1 : lastLayer));

            for (int layer = firstLayer; layer <= lastLayer; layer++)
                for (uint row = 0; row < (layer + 1); row++)
                    for (uint column = 0; column < (layer + 1); column++)
                        action(chunks[layer][row, column].FindComponent<TerrainChunk>());
        }

        private void UpdateChunkLocations()
        {
            if (center == null || chunks == null || baseChunk == null)
                return;

            baseChunk.CenterLocation = center;
            Chunks((chunk) =>
            {




            }, 1);
            


        }
    }
}
