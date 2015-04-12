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
	public class MapTile : MapBehavior
	{
		internal const float BASE_SIZE = 2048.0f;
		
		private readonly MapTile[] children;
		private readonly MapTile parent;
		private readonly Tile source;
		private BasicMaterial matte, texture;
		private Vector3 northWest, southEast;
		private float size;
		private MaterialsMap materialsMap;
		private volatile bool changingTexture = false;
		private const float LAYER_SPACING = 0.1f;
		private BoxCollider boxCollider;
		private static readonly object texLock = new object();

		/////////////////////////////////////////////////////////////////////
		// PROPERTIES
		/////////////////////////////////////////////////////////////////////

		internal MapTile Root
		{
			get { return parent == null ? this : parent.Root; }
		}

		internal MapTile Parent
		{
			get { return parent; }
		}

		internal MapTile[] Children
		{
			get { return children; }
		}

		internal uint Level
		{
			get { return source.Level; }
		}

		internal uint Row
		{
			get { return source.Row; }
		}

		internal uint Column
		{
			get { return source.Column; }
		}

		internal Tile Source
		{
			get { return source; }
		}

		internal float Size
		{
			get { return size; }
		}		

		internal bool Textured
		{
			get { return texture != null && !changingTexture; }
		}

		internal bool AllChildrenTextured
		{
			get
			{
				return children == null ? Textured :
					children[0].Textured && children[1].Textured
					&& children[2].Textured && children[3].Textured;
			}
		}

		/////////////////////////////////////////////////////////////////////
		// CONSTRUCTORS
		/////////////////////////////////////////////////////////////////////

		public static Entity Create()
		{
			return Create(null, 0);
		}

		internal static Entity Create(MapTile parent, uint childIndex)
		{
			//tile hierarchy
			uint level = parent == null ? 0 : parent.Level + 1;
			uint row = parent == null ? 0 : (parent.Row * 2) + (childIndex / 2);
			uint column = parent == null ? 0 : (parent.Column * 2) + (childIndex % 2);
			Tile source = (WFUApplication.MainForm as EyeMainForm).BaseTile.FindChild(level, row, column);
			if (source == null)
				return null;
			MapTile tile = new MapTile(source, parent);
			if (parent != null)
				parent.children[childIndex] = tile;
			tile.size = BASE_SIZE / (float)Math.Pow(2.0f, level);

			Vector3 position = parent == null ?  Vector3.Zero : parent.Transform3D.Position;
			Vector3 localPosition = parent == null ? Vector3.Zero : new Vector3(
				(tile.size / -2.0f) + (tile.size * (childIndex % 2)),
				LAYER_SPACING,
				(tile.size / -2.0f) + (tile.size * (childIndex / 2)));

			tile.northWest = localPosition - new Vector3(tile.size / 2.0f, 0.0f, tile.size / 2.0f);
			tile.southEast = localPosition + new Vector3(tile.size / 2.0f, 0.0f, tile.size / 2.0f);

			//entity
			Entity entity = new Entity()
				.AddComponent(tile.Transform3D = new Transform3D()
				{
					Position = position,
					LocalPosition = localPosition
				})
				.AddComponent(Model.CreatePlane(Vector3.UnitY, tile.size))
				.AddComponent(tile.materialsMap = new MaterialsMap(tile.matte = new BasicMaterial(MapScene.WhiteTexture)
				{
					LayerType = typeof(Terrain),
					DiffuseColor = (row + column) % 2 == 0 ? Color.Peru : Color.Sienna,
					Alpha = 0.0f//1.0f
				}))
				.AddComponent(new ModelRenderer())
				.AddComponent(tile.boxCollider = new BoxCollider() { DebugLineColor = Color.Brown })
				.AddComponent(tile);

			if (tile.children != null)
			{
				entity.AddChild(Create(tile,0));
				entity.AddChild(Create(tile,1));
				entity.AddChild(Create(tile,2));
				entity.AddChild(Create(tile,3));
			}
			return entity;
		}

		internal MapTile(Tile source, MapTile parent)
		{
			this.source = source;
			this.parent = parent;

			if (source.ZoomLevel >= Tile.ZoomLevelMax)
			{
				children = null;
				return;
			}
			children = new MapTile[4] { null, null, null, null };

			source.ImageStateChanged += source_ImageStateChanged;
		}

		/////////////////////////////////////////////////////////////////////
		// PUBLIC METHODS
		/////////////////////////////////////////////////////////////////////

		public Vector3 LocationToVector(ILocation loc)
		{
			if (source == null)
				return Vector3.Zero;
			return source.LocationToVector(northWest, southEast, loc);
		}

		/////////////////////////////////////////////////////////////////////
		// PROTECTED METHODS
		/////////////////////////////////////////////////////////////////////

		protected override void Initialize()
		{
			base.Initialize();
		}

		protected override void Update(TimeSpan gameTime)
		{
			float secs = (float)gameTime.TotalSeconds;

			//position
			if (parent == null)
				Transform3D.Position = Vector3.Lerp(Transform3D.Position,
					new Vector3(0.0f, -MapScene.VisibleLevel*LAYER_SPACING, 0.0f),
					secs * SCALE_SPEED);
				/*
			else if (source.ElevationState == Tile.LoadingState.Finished)
			{
				Transform3D.LocalPosition = Vector3.Lerp(Transform3D.LocalPosition,
					new Vector3(Transform3D.LocalPosition.X,
						(float)source.Elevation(source.Center.Latitude.Value, source.Center.Longitude.Value) *0.1f,
						Transform3D.LocalPosition.Z),
					secs * SCALE_SPEED);
			}
				 * */
			
			//alpha
			bool visible = MapScene.VisibleLevel == Level || (Textured && !AllChildrenTextured);
			if (texture != null && !changingTexture)
				texture.Alpha = texture.Alpha.Lerp(visible ? 1.0f : 0.0f,
					(float)gameTime.TotalSeconds * FADE_SPEED * (visible ? 1.0f : 0.5f));

			//check for need to instigate texture or elevation creation
			if (MapScene.VisibleLevel == Level
				&& Owner.Scene.RenderManager.ActiveCamera3D.Contains(boxCollider)
				&& !WiFindUs.Forms.MainForm.HasClosed)
			{
				if (!changingTexture && texture == null)
				{
					if (source.ImageState == Tile.LoadingState.Waiting)
						source.LoadImage();
					else if (source.ImageState == Tile.LoadingState.Finished)
					{
						changingTexture = true;
						ThreadPool.QueueUserWorkItem(ChangeTexture);
					}
				}

				if (parent == null && source.ElevationState == Tile.LoadingState.Waiting)
					source.LoadElevation();
			}
		}

		/////////////////////////////////////////////////////////////////////
		// PRIVATE METHODS
		/////////////////////////////////////////////////////////////////////

		private void source_ImageStateChanged(Tile source)
		{
			if (changingTexture)
				return;
			materialsMap.DefaultMaterial = matte;
			if (texture != null)
			{
				texture.Texture.Unload();
				texture = null;
			}
		}

		private void ChangeTexture(Object args)
		{
			WiFindUs.Forms.MainForm.SpinLock(texLock, 100, () =>
			{
				BasicMaterial nextMaterial;
				bool isTexture = false;
				if (source.Image == null)
					nextMaterial = matte;
				else
				{
					try
					{
						using (Stream stream = source.Image.GetStream())
							nextMaterial = new BasicMaterial(Texture2D.FromFile(RenderManager.GraphicsDevice, stream))
							{
								LayerType = typeof(Terrain)
							};
						nextMaterial.Alpha = 0.0f;
						isTexture = true;
						if (parent != null)
							source.DisposeImage();
					}
					catch
					{
						nextMaterial = matte;
						isTexture = false;
					}
				}

				materialsMap.DefaultMaterial = nextMaterial;

				if (texture != null)
				{
					texture.Texture.Unload();
					texture = null;
				}

				if (isTexture)
					texture = nextMaterial;
				changingTexture = false;
			});
		}
	}
}
