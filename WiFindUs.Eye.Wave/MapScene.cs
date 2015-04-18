using System;
using System.Collections.Generic;
using WaveEngine.Common.Graphics;
using WaveEngine.Common.Math;
using WaveEngine.Components.Cameras;
using WaveEngine.Components.Graphics3D;
using WaveEngine.Framework;
using WaveEngine.Framework.Graphics;
using WaveEngine.Framework.Physics3D;
using WaveEngine.Framework.Services;
using WiFindUs.Controls;
using WiFindUs.Eye.Wave.Adapter;
using WiFindUs.Eye.Wave.Markers;
using WiFindUs.Extensions;
using WiFindUs.Eye.Wave.Layers;
using System.IO;
using WaveEngine.Materials;
using WiFindUs.Themes;
using System.Linq;

namespace WiFindUs.Eye.Wave
{
	public class MapScene : Scene, IThemeable
	{
		public event Action<MapScene> SceneStarted;

		private Map3D hostControl;
		private FixedCamera camera;
		private BoxCollider groundPlaneCollider;
		private Terrain baseTile;
		private uint visibleLevel = 0;
		private float markerScale = 1.0f;
		private static Texture2D whiteTex = null;
		private List<DeviceMarker> deviceMarkers = new List<DeviceMarker>();
		private List<NodeMarker> nodeMarkers = new List<NodeMarker>();
		private List<NodeLinkMarker> nodeLinkMarkers = new List<NodeLinkMarker>();
		private List<DeviceLinkMarker> deviceLinkMarkers = new List<DeviceLinkMarker>();
		private List<Marker> allMarkers = new List<Marker>();
		private MapInput inputBehaviour;
		private MapCamera cameraController;
		private MapCursor cursor;

		/////////////////////////////////////////////////////////////////////
		// PROPERTIES
		/////////////////////////////////////////////////////////////////////

		internal uint VisibleLevel
		{
			get { return visibleLevel; }
			set
			{
				uint level = value.Clamp(Tile.LevelMin, Tile.LevelMax);
				if (level == visibleLevel)
					return;

				visibleLevel = level;
				//for (int i = 0; i < tileLayers.Length; i++)
					//tileLayers[i].IsVisible = tileLayers[i].IsActive = (i == visibleLayer);
			}
		}

		internal Terrain BaseTile
		{
			get { return baseTile; }
		}

		internal List<DeviceMarker> DeviceMarkers
		{
			get { return deviceMarkers; }
		}

		internal List<NodeMarker> NodeMarkers
		{
			get { return nodeMarkers; }
		}

		internal List<Marker> AllMarkers
		{
			get { return allMarkers; }
		}

		internal bool DebugMode
		{
			get { return RenderManager.DebugLines; }
			set
			{
				if (value == DebugMode)
					return;
				RenderManager.DebugLines = value;
				WaveServices.ScreenContextManager.SetDiagnosticsActive(value);
			}
		}

		internal MapInput Input
		{
			get { return inputBehaviour; }
		}

		internal MapCamera Camera
		{
			get { return cameraController; }
		}

		internal MapCursor Cursor
		{
			get { return cursor; }
		}

		internal BoxCollider GroundPlane
		{
			get { return groundPlaneCollider; }
		}

		internal float MarkerScale
		{
			get { return markerScale; }
			set
			{
				if (value.Tolerance(markerScale, 0.01f))
					return;
				markerScale = value;
			}
		}

		internal static Texture2D WhiteTexture
		{
			get { return whiteTex; }
			private set { whiteTex = value; }
		}

		internal int BackBufferWidth
		{
			get { return hostControl.BackBufferWidth; }
		}

		internal int BackBufferHeight
		{
			get { return hostControl.BackBufferHeight; }
		}

		internal ISelectableGroup SelectionGroup
		{
			get { return hostControl.SelectionGroup; }
		}

		/////////////////////////////////////////////////////////////////////
		// CONSTRUCTORS
		/////////////////////////////////////////////////////////////////////

		public MapScene(Map3D hostControl)
		{
			if (hostControl == null)
				throw new ArgumentNullException("hostControl", "MapScene cannot be instantiated outside of a host MapControl.");
			this.hostControl = hostControl;
		}

		/////////////////////////////////////////////////////////////////////
		// PUBLIC METHODS
		/////////////////////////////////////////////////////////////////////

		public virtual void ApplyTheme(ITheme theme)
		{
			if (theme == null || camera == null)
				return;

			camera.BackgroundColor = theme.Background.Dark.Colour.Wave();
		}

		public Vector3 LocationToVector(ILocation loc)
		{
			if (baseTile == null)
				return Vector3.Zero;
			return baseTile.LocationToVector(loc);
		}

		public ILocation VectorToLocation(Vector3 vec)
		{
			if (baseTile == null || baseTile.Source == null)
				return WiFindUs.Eye.Location.EMPTY;

			return new Location(
				baseTile.Source.NorthWest.Latitude - ((vec.Z - (baseTile.Size / -2f)) / baseTile.Size) * baseTile.Source.LatitudinalSpan,
				baseTile.Source.NorthWest.Longitude + ((vec.X - (baseTile.Size / -2f)) / baseTile.Size) * baseTile.Source.LongitudinalSpan
				);
		}

		public NodeMarker GetNodeMarker(Node node)
		{
			if (node == null)
				return null;
			return (from mk in nodeMarkers
				   where mk.Entity == node
				   select mk).FirstOrDefault();
		}

		public NodeMarker GetNodeMarker(uint nodeNumber)
		{
			if (nodeNumber == 0 || nodeNumber > 254)
				return null;

			return (from mk in nodeMarkers
					where mk.Entity.Number.GetValueOrDefault() == nodeNumber
					select mk).FirstOrDefault();
		}

		public DeviceMarker GetDeviceMarker(Device device)
		{
			if (device == null)
				return null;

			return (from mk in deviceMarkers
					where mk.Entity == device
					select mk).FirstOrDefault();
		}

		public NodeLinkMarker GetNodeLinkMarker(NodeMarker nodeA, NodeMarker nodeB)
		{
			if (nodeA == null || nodeB == null)
				return null;

			return (from mk in nodeLinkMarkers
					where mk.LinksMarkers(nodeA, nodeB)
					select mk
					).FirstOrDefault();
		}

		/////////////////////////////////////////////////////////////////////
		// PROTECTED METHODS
		/////////////////////////////////////////////////////////////////////

		protected override void CreateScene()
		{
			//create basic white texture
			Texture2D tex;
			using (System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(8,8))
			{
				using (System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(bmp))
					g.Clear(System.Drawing.Color.White);
				using (Stream stream = bmp.GetStream())
					tex = Texture2D.FromFile(RenderManager.GraphicsDevice, stream);
			}
			WhiteTexture = tex;
		
			//add custom layers
			RenderManager.RegisterLayerAfter(new TerrainLayer(this.RenderManager), DefaultLayers.Opaque);
			RenderManager.RegisterLayerAfter(new NonPremultipliedAlpha(this.RenderManager), typeof(TerrainLayer));
			RenderManager.RegisterLayerAfter(new Overlays(this.RenderManager), typeof(NonPremultipliedAlpha));
			RenderManager.RegisterLayerAfter(new Wireframes(this.RenderManager), typeof(Overlays));

			//set up camera
			Debugger.V("MapScene: initializing camera");
			camera = new FixedCamera("camera", Vector3.Up * 200.0f, Vector3.Zero)
			{
				NearPlane = 1f,
				FarPlane = MapCamera.MAX_ZOOM * 3.0f,
				ClearFlags = ClearFlags.All,
			};
			camera.Entity.AddComponent(cameraController = new WiFindUs.Eye.Wave.MapCamera());
			EntityManager.Add(camera);
			RenderManager.SetFrustumCullingCamera(camera.Entity);

			//create global lighting
			Debugger.V("MapScene: creating lighting");
			Vector3 sun = new Vector3(0f, 100f, 35f);
			sun.Normalize();
			DirectionalLight skylight = new DirectionalLight("SkyLight", sun)
			{
				Color = Color.Gray
			};
			EntityManager.Add(skylight);

			//create terrain tiles
			Debugger.V("MapScene: creating tiles");
			Entity tileEntity = Terrain.Create();
			baseTile = tileEntity.FindComponent<Terrain>();
			EntityManager.Add(tileEntity);

			//create ground plane
			Debugger.V("MapScene: creating ground plane");
			EntityManager.Add(new Entity()
				.AddComponent(new Transform3D() { Position = new Vector3(0f, 0f, 0f) })
				.AddComponent(Model.CreatePlane(Vector3.UnitY, Terrain.SIZE * 100f))
				.AddComponent(groundPlaneCollider = new BoxCollider() { DebugLineColor = Color.Red }));

			//add scene behaviours
			Debugger.V("MapScene: creating behaviours");
			AddSceneBehavior(inputBehaviour = new MapInput(hostControl), SceneBehavior.Order.PostUpdate);

			//apply theme
			ApplyTheme(Theme.Current);
			Theme.ThemeChanged += ApplyTheme;

			//create cursor
			Entity cursorEntity = MapCursor.Create();
			cursor = cursorEntity.FindComponent<MapCursor>();
			EntityManager.Add(cursorEntity);
		}

		protected override void Start()
		{
			base.Start();
			//Tiles((tile) => tile.CalculatePosition());
			EyeMainForm eyeForm = (WFUApplication.MainForm as EyeMainForm);

			//load existing devices
			foreach (Device device in eyeForm.Devices)
			{
				if (!device.Loaded)
					continue;
				Device_OnDeviceLoaded(device);
			}
			Device.OnDeviceLoaded += Device_OnDeviceLoaded;

			//load existing nodes
			foreach (Node node in eyeForm.Nodes)
			{
				if (!node.Loaded)
					continue;
				Node_OnNodeLoaded(node);
			}
			Node.OnNodeLoaded += Node_OnNodeLoaded;

			//load existing node links
			foreach (NodeLink nodeLink in eyeForm.NodeLinks)
			{
				if (!nodeLink.Loaded)
					continue;
				NodeLink_OnNodeLinkLoaded(nodeLink);
			}
			NodeLink.OnNodeLinkLoaded += NodeLink_OnNodeLinkLoaded;

			//start scene
			if (SceneStarted != null)
				SceneStarted(this);
		}

		/////////////////////////////////////////////////////////////////////
		// PRIVATE METHODS
		/////////////////////////////////////////////////////////////////////

		private void Device_OnDeviceLoaded(Device device)
		{
			//device entity
			Entity entity = DeviceMarker.Create(device);
			DeviceMarker marker = entity.FindComponent<DeviceMarker>();
			deviceMarkers.Add(marker);
			allMarkers.Add(marker);
			EntityManager.Add(entity);
			device.SelectionGroup = SelectionGroup;

			//device link
			entity = LinkMarker.Create(marker, null, typeof(DeviceLinkMarker));
			DeviceLinkMarker linkMarker = entity.FindComponent<DeviceLinkMarker>();
			linkMarker.Diameter = 0.5f;
			linkMarker.Colour = Color.Lime;
			deviceLinkMarkers.Add(linkMarker);
			allMarkers.Add(linkMarker);
			EntityManager.Add(entity);
		}

		private void Node_OnNodeLoaded(Node node)
		{
			//node entity
			Entity entity = NodeMarker.Create(node);
			NodeMarker marker = entity.FindComponent<NodeMarker>();
			nodeMarkers.Add(marker);
			allMarkers.Add(marker);
			EntityManager.Add(entity);
			node.SelectionGroup = SelectionGroup;
		}

		private void NodeLink_OnNodeLinkLoaded(NodeLink nodeLink)
		{
			if (nodeLink == null
				|| nodeLink.Start == null
				|| nodeLink.End == null)
				return;

			NodeMarker A = GetNodeMarker(nodeLink.Start);
			NodeMarker B = GetNodeMarker(nodeLink.End);
			if (A == null || B == null)
				return;

			NodeLinkMarker link = GetNodeLinkMarker(A, B);
			if (link != null)
				return;

			Entity entity = LinkMarker.Create(A, B, typeof(NodeLinkMarker));
			link = entity.FindComponent<NodeLinkMarker>();
			nodeLinkMarkers.Add(link);
			allMarkers.Add(link);
			link.NodeLink = nodeLink;
			EntityManager.Add(entity);
		}

		/*
		private void CreateTileLayer(uint layer)
		{
			Entity layerEntity = tileLayers[layer];
			if (layerEntity == null)
			{
				tileLayers[layer] = layerEntity = new Entity()
					.AddComponent(new Transform3D() { Position = new Vector3(0f, 0f, 0f) });
				EntityManager.Add(layerEntity);
			}

			int depth = 1 << (int)layer;
			Entity[,] layerTiles = tiles[layer];
			if (layerTiles == null)
				tiles[layer] = layerTiles = new Entity[depth, depth];
			for (uint row = 0; row < depth; row++)
			{
				for (uint column = 0; column < depth; column++)
				{
					Entity tileEntity = layerTiles[row, column];
					if (tileEntity == null)
					{
						layerTiles[row, column] = tileEntity = TerrainTile.Create(layer, row, column, baseTile);
						layerEntity.AddChild(tileEntity);
						if (baseTile == null && layer == 0)
							baseTile = tileEntity.FindComponent<TerrainTile>();
					}
				}
			}
		}
		 * */

		/*
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
		 * */
	}
}
