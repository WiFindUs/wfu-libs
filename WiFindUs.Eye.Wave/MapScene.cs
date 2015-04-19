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
		private Terrain terrain;
		private float markerScale = 1.0f;
		private static Texture2D whiteTex = null;
		private readonly List<Marker> markers = new List<Marker>();
		private MapInput inputBehaviour;
		private MapCamera cameraController;
		private MapCursor cursor;
		private readonly Plane groundPlane;

		/////////////////////////////////////////////////////////////////////
		// PROPERTIES
		/////////////////////////////////////////////////////////////////////

		internal Terrain Terrain
		{
			get { return terrain; }
		}

		internal List<Marker> Markers
		{
			get { return markers; }
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

		internal Plane GroundPlane
		{
			get { return groundPlane; }
		}

		/////////////////////////////////////////////////////////////////////
		// CONSTRUCTORS
		/////////////////////////////////////////////////////////////////////

		public MapScene(Map3D hostControl)
		{
			if (hostControl == null)
				throw new ArgumentNullException("hostControl", "MapScene cannot be instantiated outside of a host MapControl.");
			this.hostControl = hostControl;
			this.groundPlane = new Plane(Vector3.Up, 0.0f);
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
			if (terrain == null)
				return Vector3.Zero;
			return terrain.LocationToVector(loc);
		}

		public ILocation VectorToLocation(Vector3 vec)
		{
			if (terrain == null)
				return WiFindUs.Eye.Location.EMPTY;
			return terrain.VectorToLocation(vec);
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
			RenderManager.RegisterLayerAfter(new NonPremultipliedAlpha(this.RenderManager), DefaultLayers.Opaque);
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
			DirectionalLight skylight = new DirectionalLight("SkyLight", new Vector3(1));
			EntityManager.Add(skylight);

			//create terrain tiles
			Debugger.V("MapScene: creating tiles");
			Entity tileEntity = Terrain.Create();
			terrain = tileEntity.FindComponent<Terrain>();
			EntityManager.Add(tileEntity);

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
			markers.Add(marker);
			EntityManager.Add(entity);
			device.SelectionGroup = SelectionGroup;

			//device link
			entity = LinkMarker.Create(marker, null, typeof(DeviceLinkMarker));
			DeviceLinkMarker linkMarker = entity.FindComponent<DeviceLinkMarker>();
			linkMarker.Diameter = 0.5f;
			linkMarker.Colour = Color.Lime;
			markers.Add(linkMarker);
			EntityManager.Add(entity);
		}

		private void Node_OnNodeLoaded(Node node)
		{
			//node entity
			Entity entity = NodeMarker.Create(node);
			NodeMarker marker = entity.FindComponent<NodeMarker>();
			markers.Add(marker);
			EntityManager.Add(entity);
			node.SelectionGroup = SelectionGroup;
		}

		private void NodeLink_OnNodeLinkLoaded(NodeLink nodeLink)
		{
			if (nodeLink == null
				|| nodeLink.Start == null
				|| nodeLink.End == null)
				return;

			NodeMarker A = markers.OfType<NodeMarker>().Where(mk => mk.Entity == nodeLink.Start).FirstOrDefault();
			NodeMarker B = markers.OfType<NodeMarker>().Where(mk => mk.Entity == nodeLink.End).FirstOrDefault();
			if (A == null || B == null)
				return;

			NodeLinkMarker link = markers.OfType<NodeLinkMarker>().Where(mk => mk.LinksMarkers(A, B)).FirstOrDefault();
			if (link != null)
				return;

			Entity entity = LinkMarker.Create(A, B, typeof(NodeLinkMarker));
			link = entity.FindComponent<NodeLinkMarker>();
			markers.Add(link);
			link.NodeLink = nodeLink;
			EntityManager.Add(entity);
		}
	}
}
