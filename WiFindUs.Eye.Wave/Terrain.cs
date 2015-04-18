using System;
using System.Collections.Generic;
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
using WiFindUs.Eye.Wave.Layers;
using WiFindUs.Extensions;
using System.IO;
using System.Threading;

namespace WiFindUs.Eye.Wave
{
	public class Terrain : MapBehavior
	{
		internal const float SIZE = 2048.0f;
		internal const int SUBDIVISIONS = 62;
		internal const float UPDATE_INTERVAL = 3.0f;
		
		private readonly BaseTile source;
		private BasicMaterial matte;
		private Vector3 northWest, southEast;
		private float size;
		private MaterialsMap materialsMap;
		private BoxCollider boxCollider;
		private PolyPlane plane = null;
		private float meterSize = 0.0f;
		private volatile bool updatingTexture = false;
		private volatile float textureTimer = 0.0f;
		private volatile bool pendingTextureUpdate = false;
		private volatile bool updatingElevation = false;
		private volatile bool pendingElevationUpdate = false;

		/////////////////////////////////////////////////////////////////////
		// PROPERTIES
		/////////////////////////////////////////////////////////////////////

		internal BaseTile Source
		{
			get { return source; }
		}

		internal float Size
		{
			get { return size; }
		}

		internal float MeterSize
		{
			get { return meterSize; }
		}	

		/////////////////////////////////////////////////////////////////////
		// CONSTRUCTORS/INITIALIZERS
		/////////////////////////////////////////////////////////////////////

		public static Entity Create()
		{
			BaseTile source = (WFUApplication.MainForm as EyeMainForm).BaseTile;
			if (source == null)
				throw new InvalidOperationException("EyeMainForm's BaseTile was null");

			Terrain terrain = new Terrain(source);
			terrain.size = SIZE;
			terrain.meterSize = SIZE / (float)source.Width;

			terrain.northWest = new Vector3(terrain.size / -2.0f, 0.0f, terrain.size / -2.0f);
			terrain.southEast = new Vector3(terrain.size / 2.0f, 0.0f, terrain.size / 2.0f);

			//entity
			Entity entity = new Entity()
				.AddComponent(terrain.Transform3D = new Transform3D()
				{
					Position = Vector3.Zero,
					LocalPosition = Vector3.Zero
				})
				.AddComponent(terrain.plane = new PolyPlane(Vector3.Up, terrain.size, SUBDIVISIONS))
				.AddComponent(new PolyPlaneRenderer())
				.AddComponent(terrain.materialsMap = new MaterialsMap(terrain.matte = new BasicMaterial(MapScene.WhiteTexture)
				{
					LayerType = typeof(TerrainLayer),
					DiffuseColor = Color.Peru,
					Alpha = 1.0f
				}))
				.AddComponent(new MeshCollider())
				.AddComponent(terrain);

			return entity;
		}

		internal Terrain(BaseTile source)
		{
			this.source = source;
		}

		protected override void Initialize()
		{
#if DEBUG
			Debugger.T("enter");
#endif
			base.Initialize();

			source.ElevationStateChanged += source_ElevationStateChanged;
			source.CompositeImageChanged += source_CompositeImageChanged;
			
			if (source.ThreadlessState)
				source.Load();
#if DEBUG
			Debugger.T("exit");
#endif
		}

		/////////////////////////////////////////////////////////////////////
		// PUBLIC METHODS
		/////////////////////////////////////////////////////////////////////

		public Vector3 LocationToVector(ILocation loc)
		{
			if (source == null)
				return Vector3.Zero;
			Vector3 output = source.LocationToVector(northWest, southEast, loc);
			if (loc.Altitude.HasValue)
				output.Y = meterSize * (float)loc.Altitude.Value;
			return output;
		}

		/////////////////////////////////////////////////////////////////////
		// PROTECTED METHODS
		/////////////////////////////////////////////////////////////////////

		protected override void Update(TimeSpan gameTime)
		{
			float secs = (float)gameTime.TotalSeconds;

			if (pendingElevationUpdate && !updatingElevation)
				UpdateElevation();

			if (pendingTextureUpdate)
			{
				textureTimer += secs;
				if (!updatingTexture && textureTimer >= UPDATE_INTERVAL)
					UpdateTexture();
			}
		}

		/////////////////////////////////////////////////////////////////////
		// PRIVATE METHODS
		/////////////////////////////////////////////////////////////////////

		private void source_ElevationStateChanged(BaseTile source)
		{
			if (updatingElevation)
				pendingElevationUpdate = true;
			else
				UpdateElevation();
		}

		private void UpdateElevation()
		{
			if (updatingElevation)
				return;
			updatingElevation = true;
			pendingElevationUpdate = false;
			Thread thread = new Thread(new ThreadStart(UpdateElevationThread));
			thread.IsBackground = true;
			thread.Start();
		}

		private void UpdateElevationThread()
		{
			Thread.Sleep(500);
			if (source.ElevationState == Tile.LoadingState.Finished)
			{
				double latStep = source.LatitudinalSpan / (double)(plane.Subdivisions + 1);
				double longStep = source.LongitudinalSpan / (double)(plane.Subdivisions + 1);
				lock (source.ElevationLock)
				{
					for (uint row = 0; row < plane.Subdivisions + 2; row++)
					{
						for (uint column = 0; column < plane.Subdivisions + 2; column++)
						{
							plane.SetVertexPosition(row, column,
								meterSize * (float)source.Elevation(
									source.NorthWest.Latitude.Value - latStep * (double)(plane.Subdivisions + 1 - row),
									source.NorthWest.Longitude.Value + longStep * (double)column));
						}
					}
				}
			}
			else
			{
				for (uint row = 0; row < plane.Subdivisions + 2; row++)
					for (uint column = 0; column < plane.Subdivisions + 2; column++)
						plane.SetVertexPosition(row, column, 0.0f);
			}
			plane.FlushVertices();
			updatingElevation = false;
		}

		private void source_CompositeImageChanged(BaseTile obj)
		{
			if (updatingTexture || textureTimer < UPDATE_INTERVAL)
				pendingTextureUpdate = true;
			else
				UpdateTexture();
		}

		private void UpdateTexture()
		{
			if (updatingTexture)
				return;
			updatingTexture = true;
			pendingTextureUpdate = false;
			Thread thread = new Thread(new ThreadStart(UpdateTextureThread));
			thread.IsBackground = true;
			thread.Start();
		}

		private void UpdateTextureThread()
		{
			BasicMaterial nextTexture;

			lock (source.CompositeLock)
			{
				//http://forum.waveengine.net/forum/general/4339-how-to-update-and-draw-the-texture-at-runtime
				//might be of use to speed this up?
				//also this: http://bobpowell.net/lockingbits.aspx
				using (Stream stream = source.Composite.GetStream())
				{
					nextTexture = new BasicMaterial(Texture2D.FromFile(RenderManager.GraphicsDevice, stream))
					{
						LayerType = typeof(TerrainLayer)
					};
				}
			}

			materialsMap.DefaultMaterial = nextTexture;
			updatingTexture = false;
			textureTimer = 0.0f;
		}
	}
}
