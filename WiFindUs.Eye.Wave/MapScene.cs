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
using WiFindUs.Eye.Wave.Controls;
using WiFindUs.Eye.Wave.Markers;
using WiFindUs.Extensions;
using WiFindUs.Eye.Wave.Layers;
using System.IO;
using WaveEngine.Materials;
using WiFindUs.Themes;

namespace WiFindUs.Eye.Wave
{
	public class MapScene : Scene, IThemeable
	{
		public event Action<MapScene> SceneStarted;
		public event Action<MapScene> CenterLocationChanged;

		private const float ZOOM_RATE = 1.0f;
		private const float TILT_RATE = 1.0f;
		public const uint MIN_LEVEL = Region.GOOGLE_MAPS_TILE_MIN_ZOOM + 1;

		private MapGame hostGame;
		private EyeMainForm eyeForm;
		private FixedCamera camera;
		private ILocation center;
		private Entity[] tileLayers;
		private Entity[][,] tiles;
		private Entity groundPlane;
		private BoxCollider groundPlaneCollider;
		private TerrainTile baseTile;
		private uint visibleLayer = uint.MaxValue;
		private float markerScale = 1.0f;
		private static Texture2D whiteTex = null;

		private List<DeviceMarker> deviceMarkers = new List<DeviceMarker>();
		private List<NodeMarker> nodeMarkers = new List<NodeMarker>();
		private List<NodeLinkMarker> nodeLinkMarkers = new List<NodeLinkMarker>();
		private List<DeviceLinkMarker> deviceLinkMarkers = new List<DeviceLinkMarker>();
		private List<Marker> allMarkers = new List<Marker>();
		private MapSceneInput inputBehaviour;
		private MapSceneCamera cameraController;

		/////////////////////////////////////////////////////////////////////
		// PROPERTIES
		/////////////////////////////////////////////////////////////////////

		public ILocation CenterLocation
		{
			get { return center; }
			set
			{
				if (value == null || WiFindUs.Eye.Location.Equals(center, value))
					return;
				Debugger.I("Setting map center to " + value.ToString());

				center = value;
				baseTile.CenterLocation = value;
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

				if (CenterLocationChanged != null)
					CenterLocationChanged(this);
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
				uint layer = value >= tileLayers.Length ? (uint)tileLayers.Length - 1 : value;
				if (layer == visibleLayer)
					return;

				visibleLayer = layer;
				for (int i = 0; i < tileLayers.Length; i++)
				{ 
					bool vis = 
					tileLayers[i].SetActive();IsVisible = tileLayers[i].IsActive = (i == visibleLayer);
			}
		}

		public uint LayerCount
		{
			get { return (uint)tileLayers.Length; }
		}

		public TerrainTile BaseTile
		{
			get { return baseTile; }
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

		public List<DeviceMarker> DeviceMarkers
		{
			get { return deviceMarkers; }
		}

		public List<NodeMarker> NodeMarkers
		{
			get { return nodeMarkers; }
		}

		public List<Marker> AllMarkers
		{
			get { return allMarkers; }
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

		public MapSceneInput InputBehaviour
		{
			get { return inputBehaviour; }
		}

		public MapSceneCamera CameraController
		{
			get { return cameraController; }
		}

		public BoxCollider GroundPlane
		{
			get { return groundPlaneCollider; }
		}

		public float MarkerScale
		{
			get { return markerScale; }
			set
			{
				if (value.Tolerance(markerScale, 0.01f))
					return;
				markerScale = value;
			}
		}

		public static Texture2D WhiteTexture
		{
			get { return whiteTex; }
			private set { whiteTex = value; }
		}

		/////////////////////////////////////////////////////////////////////
		// CONSTRUCTORS
		/////////////////////////////////////////////////////////////////////

		public MapScene(MapGame hostGame)
		{
			if (hostGame == null)
				throw new ArgumentNullException("hostGame", "MapScene cannot be instantiated outside of a host MapGame.");
			this.hostGame = hostGame;
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
			if (baseTile == null || baseTile.Region == null)
				return WiFindUs.Eye.Location.EMPTY;

			return new Location(
				baseTile.Region.NorthWest.Latitude - ((vec.Z - (baseTile.Size / -2f)) / baseTile.Size) * baseTile.Region.LatitudinalSpan,
				baseTile.Region.NorthWest.Longitude + ((vec.X - (baseTile.Size / -2f)) / baseTile.Size) * baseTile.Region.LongitudinalSpan
				);
		}

		public void CancelThreads()
		{
			Tiles((tile) => tile.CancelThreads());
		}

		public NodeMarker GetNodeMarker(Node node)
		{
			if (node == null)
				return null;

			NodeMarker marker = null;
			foreach (NodeMarker mk in nodeMarkers)
			{
				if (mk.Entity == node)
				{
					marker = mk;
					break;
				}
			}
			return marker;
		}

		public NodeMarker GetNodeMarker(uint nodeNumber)
		{
			if (nodeNumber == 0 || nodeNumber > 254)
				return null;

			NodeMarker marker = null;
			foreach (NodeMarker mk in nodeMarkers)
			{
				if (mk.Entity.Number.GetValueOrDefault() == nodeNumber)
				{
					marker = mk;
					break;
				}
			}
			return marker;
		}

		public DeviceMarker GetDeviceMarker(Device device)
		{
			if (device == null)
				return null;

			DeviceMarker marker = null;
			foreach (DeviceMarker mk in deviceMarkers)
			{
				if (mk.Entity == device)
				{
					marker = mk;
					break;
				}
			}
			return marker;
		}

		public NodeLinkMarker GetNodeLinkMarker(NodeMarker nodeA, NodeMarker nodeB)
		{
			if (nodeA == null || nodeB == null)
				return null;

			NodeLinkMarker marker = null;
			foreach (NodeLinkMarker mk in nodeLinkMarkers)
			{
				if (mk.LinksMarkers(nodeA, nodeB))
				{
					marker = mk;
					break;
				}
			}
			return marker;
		}

		/////////////////////////////////////////////////////////////////////
		// PROTECTED METHODS
		/////////////////////////////////////////////////////////////////////

		protected override void CreateScene()
		{
			//load basic white texture
			WhiteTexture = RenderManager.GraphicsDevice.Load("textures/white.png");
			
			//add custom layers
			RenderManager.RegisterLayerAfter(new NonPremultipliedAlpha(this.RenderManager), DefaultLayers.Opaque);
			RenderManager.RegisterLayerAfter(new Overlays(this.RenderManager), typeof(NonPremultipliedAlpha));
			RenderManager.RegisterLayerAfter(new Wireframes(this.RenderManager), typeof(Overlays));

			//set up camera
			Debugger.V("MapScene: initializing camera");
			camera = new FixedCamera("camera", Vector3.Up * 200.0f, Vector3.Zero)
			{
				NearPlane = 1f,
				FarPlane = 10000.0f,
				ClearFlags = ClearFlags.All,
			};
			camera.Entity.AddComponent(cameraController = new WiFindUs.Eye.Wave.MapSceneCamera());
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

			//create terrain layers
			Debugger.V("MapScene: creating layers");
			tileLayers = new Entity[Region.GOOGLE_MAPS_TILE_MAX_ZOOM - MIN_LEVEL + 1];
			tiles = new Entity[tileLayers.Length][,];
			for (uint layer = 0; layer < tiles.Length; layer++)
				CreateTileLayer(layer);

			//create ground plane
			Debugger.V("MapScene: creating ground plane");
			groundPlane = new Entity()
				.AddComponent(new Transform3D() { Position = new Vector3(0f, 0f, 0f) })
				.AddComponent(Model.CreatePlane(Vector3.UnitY, baseTile.Size * 100f))
				.AddComponent(groundPlaneCollider = new BoxCollider() { DebugLineColor = Color.Red });
			EntityManager.Add(groundPlane);

			//add scene behaviours
			Debugger.V("MapScene: creating behaviours");
			AddSceneBehavior(inputBehaviour = new MapSceneInput(), SceneBehavior.Order.PostUpdate);

			//apply theme
			ApplyTheme(Theme.Current);
			Theme.ThemeChanged += ApplyTheme;
		}

		protected override void Start()
		{
			base.Start();
			Tiles((tile) => tile.CalculatePosition());
			VisibleLayer = 0;
			eyeForm = (WFUApplication.MainForm as EyeMainForm);

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

			//device link
			entity = LinkMarker.Create(marker, null, typeof(DeviceLinkMarker));
			DeviceLinkMarker linkMarker = entity.FindComponent<DeviceLinkMarker>();
			linkMarker.Diameter = 0.5f;
			linkMarker.ToSecondaryPoint = true;
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

		private void CreateTileLayer(uint layer)
		{
			Entity layerEntity = tileLayers[layer];
			if (layerEntity == null)
			{
				tileLayers[layer] = layerEntity = new Entity()
				{
					IsActive = false,
					IsVisible = false
				}
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
	}
}
