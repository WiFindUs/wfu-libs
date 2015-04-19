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
		public const float SIZE = 2048.0f;
		public const int RESOLUTION = 7; //subdivisions = (2 ^ RESOLUTION)-2
		internal const float UPDATE_INTERVAL = 3.0f;
		
		private readonly Map source;
		private BasicMaterial textureMaterial;
		private Texture2D texture;
		private byte[][][] textureData;
		private Vector3 northWest, southEast;
		private MaterialsMap materialsMap;
		private PolyPlane plane = null;
		private float meterLength = 0.0f;
		private volatile bool updatingTexture = false;
		private volatile float textureTimer = 0.0f;
		private volatile bool pendingTextureUpdate = false;
		private volatile bool updatingElevation = false;
		private volatile bool pendingElevationUpdate = false;
		private Ray ray;

		/////////////////////////////////////////////////////////////////////
		// PROPERTIES
		/////////////////////////////////////////////////////////////////////

		internal Map Source
		{
			get { return source; }
		}

		internal float MeterLength
		{
			get { return meterLength; }
		}

		internal PolyPlane Plane
		{
			get { return plane; }
		}

		/////////////////////////////////////////////////////////////////////
		// CONSTRUCTORS/INITIALIZERS
		/////////////////////////////////////////////////////////////////////

		public static Entity Create()
		{
			Map source = (WFUApplication.MainForm as EyeMainForm).Map;
			if (source == null)
				throw new InvalidOperationException("EyeMainForm's Map was null");

			Terrain terrain = new Terrain(source);
			
			//entity
			Entity entity = new Entity()
				.AddComponent(terrain.Transform3D = new Transform3D()
				{
					Position = Vector3.Zero,
					LocalPosition = Vector3.Zero
				})
				.AddComponent(terrain.plane = new PolyPlane(Vector3.Up, SIZE, (1 << RESOLUTION) - 2))
				.AddComponent(new PolyPlaneRenderer())
				.AddComponent(terrain.materialsMap = new MaterialsMap(terrain.textureMaterial))
				.AddComponent(terrain);

			return entity;
		}

		internal Terrain(Map source)
		{
			this.source = source;
			meterLength = SIZE / (float)source.Width;
			northWest = new Vector3(SIZE / -2.0f, 0.0f, SIZE / -2.0f);
			southEast = new Vector3(SIZE / 2.0f, 0.0f, SIZE / 2.0f);

			//create texture
			texture = new Texture2D()
			{
				Format = PixelFormat.R8G8B8A8,
				Width = Map.COMPOSITE_SIZE,
				Height = Map.COMPOSITE_SIZE,
				Levels = 1
			};
			textureData = new byte[1][][]; // only 1 texture part
			textureData[0] = new byte[1][]; // 1 mipmap level
			textureData[0][0] = new byte[texture.Width * texture.Height * 4]; // texture data size is ( width * height * bytesperpixel )
			for (int i = 0; i < texture.Width * texture.Height; i++)
			{
				textureData[0][0][i * 4] = Color.Peru.R;		//red
				textureData[0][0][i * 4 + 1] = Color.Peru.G;	//green
				textureData[0][0][i * 4 + 2] = Color.Peru.B;	//blue
				textureData[0][0][i * 4 + 3] = 255;	//alpha
			}
			texture.Data = textureData;

			//create basic material shader
			textureMaterial = new BasicMaterial(texture)
			{
				LayerType = DefaultLayers.Opaque,
				LightingEnabled = true,
				AmbientLightColor = Color.White * 0.05f
			};

			//location altitude ray
			ray = new Ray();
			Vector3 dir = new Vector3(0.001f, -1.0f, 0.001f);
			dir.Normalize();
			ray.Direction = dir;
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
			Vector3 output = source.LocationToVector(northWest, southEast, loc);
			if (source.ElevationState == Tile.LoadingState.Finished && source.Contains(loc))
			{
				ray.Position = new Vector3(output.X, plane.BoundingBox.Max.Y + 20.0f, output.Z);
				Vector3 normal;
				float? result = Intersects(ref ray, out normal);
				if (result.HasValue)
					output.Y = (ray.Position + ray.Direction * result.Value).Y;
			}
			
			return output;
		}

		public ILocation VectorToLocation(Vector3 vec)
		{
			return new Location(
				source.NorthWest.Latitude - ((vec.Z - (Terrain.SIZE / -2f)) / Terrain.SIZE) * source.LatitudinalSpan,
				source.NorthWest.Longitude + ((vec.X - (Terrain.SIZE / -2f)) / Terrain.SIZE) * source.LongitudinalSpan,
				null, //accuracy
				Map.ELEV_MIN + Map.ELEV_RANGE * (vec.Y / (Map.ELEV_RANGE * meterLength))
				);
		}

		public float? Intersects(ref Ray ray, out Vector3 normal)
		{
			return plane.Intersects(ref ray, out normal);
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

		private void source_ElevationStateChanged(Map source)
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
			
			//update vertex positions
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
								meterLength * (float)source.Elevation(
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

			//update vertex normals (lighting etc)
			for (uint row = 0; row < plane.Subdivisions + 2; row++)
				for (uint column = 0; column < plane.Subdivisions + 2; column++)
					plane.RecalculateVertexNormal(row, column);

			//push updates
			plane.FlushVertices();
			plane.UpdateBoundingBox();
			updatingElevation = false;
		}

		private void source_CompositeImageChanged(Map obj)
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

		private unsafe void UpdateTextureThread()
		{
			lock (source.CompositeLock)
			{			
				System.Drawing.Imaging.BitmapData bmd = source.Composite.LockBits(
					new System.Drawing.Rectangle(0, 0, Map.COMPOSITE_SIZE, Map.COMPOSITE_SIZE),
					System.Drawing.Imaging.ImageLockMode.ReadOnly,
					source.Composite.PixelFormat);

				int count = 5;
				Thread[] threads = new Thread[count-1];
				for (int i = 0; i < count-1; i++)
				{
					object[] args = new object[] { count, i, bmd };
					threads[i] = new Thread(new ParameterizedThreadStart(UpdateTextureSubThread));
					threads[i].IsBackground = true;
					threads[i].Start(args);
				}
				UpdateTextureRows(bmd, count - 1, count);
				for (int i = 0; i < count - 1; i++)
					threads[i].Join();

				source.Composite.UnlockBits(bmd);

				texture.Data = textureData;
				RenderManager.GraphicsDevice.Textures.UploadTexture(texture);
			}

			updatingTexture = false;
			textureTimer = 0.0f;
		}

		private unsafe void UpdateTextureSubThread(object argsArray)
		{
			object[] args = argsArray as object[];
			int count = (int)args[0];
			int index = (int)args[1];
			System.Drawing.Imaging.BitmapData bmd = args[2] as System.Drawing.Imaging.BitmapData;

			UpdateTextureRows(bmd, index, count);
		}

		private unsafe void UpdateTextureRows(System.Drawing.Imaging.BitmapData bmd, int rowStart, int rowStep)
		{
			for (int y = rowStart; y < Map.COMPOSITE_SIZE; y += rowStep)
			{
				byte* row = (byte*)bmd.Scan0 + (y * bmd.Stride);
				for (int x = 0; x < Map.COMPOSITE_SIZE; x++)
				{
					int ti = y * Map.COMPOSITE_SIZE + x;
					textureData[0][0][ti * 4] = row[x * 4 + 2];		//red
					textureData[0][0][ti * 4 + 1] = row[x * 4 + 1];	//green
					textureData[0][0][ti * 4 + 2] = row[x * 4];	//blue
					textureData[0][0][ti * 4 + 3] = row[x * 4 + 3];	//alpha
				}
			}
		}
	}
}
