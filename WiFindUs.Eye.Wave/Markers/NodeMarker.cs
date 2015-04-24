using System;
using WaveEngine.Common.Graphics;
using WaveEngine.Common.Math;
using WaveEngine.Components.Graphics3D;
using WaveEngine.Components.UI;
using WaveEngine.Framework;
using WaveEngine.Framework.Graphics;
using WaveEngine.Framework.Physics3D;
using WaveEngine.Framework.Services;
using WaveEngine.Framework.UI;
using WaveEngine.Materials;
using WiFindUs.Extensions;
using WiFindUs.Eye.Wave.Layers;

namespace WiFindUs.Eye.Wave.Markers
{
	public class NodeMarker : EntityMarker<Node>, ILinkableMarker
	{
		private Transform3D ringTransform, coreTransform;
		private BasicMaterial spikeMat, coreMat;
		private float fader = 0.0f;
		private Color channelColour = Node.CHANNEL_NIL_COLOR.Wave();

		/////////////////////////////////////////////////////////////////////
		// PROPERTIES
		/////////////////////////////////////////////////////////////////////

		public Vector3 LinkPoint
		{
			get { return coreTransform.Position; }
		}

		/////////////////////////////////////////////////////////////////////
		// CONSTRUCTORS
		/////////////////////////////////////////////////////////////////////

		public static NodeMarker Create(Node node)
		{
			NodeMarker marker = new NodeMarker(node);
			Entity entity = new Entity()
				//base
				.AddComponent(new Transform3D())
				.AddComponent(marker)
				.AddComponent(marker.CylindricalCollider = new CylindricalCollider(28.0f, 7.0f, 14.0f))
				.AddComponent(new CylindricalColliderRenderer(2,5))
				//spike
				.AddChild
				(
					new Entity("spike")
					.AddComponent(new Transform3D()
					{
						LocalPosition = new Vector3(0.0f, 7.0f, 0.0f),
						Rotation = new Vector3(180.0f.ToRadians(), 0f, 0f)
					})
					.AddComponent(new MaterialsMap(marker.spikeMat = new BasicMaterial(MapScene.WhiteTexture)
					{
						LayerType = typeof(NonPremultipliedAlpha),
						LightingEnabled = true,
						AmbientLightColor = Color.White * 0.75f,
						Alpha = 0.5f
					}))
					.AddComponent(Model.CreateCone(14f, 8f, 8))
					.AddComponent(new ModelRenderer())
				)
				//core
				.AddChild
				(
					new Entity("core")
					.AddComponent(marker.coreTransform = new Transform3D()
					{
						LocalPosition = new Vector3(0.0f, 21.0f, 0.0f)
					})
					.AddComponent(new MaterialsMap(marker.coreMat = new BasicMaterial(MapScene.WhiteTexture)
					{
						LayerType = typeof(NonPremultipliedAlpha),
						LightingEnabled = true,
						AmbientLightColor = Color.White * 0.75f,
						Alpha = 0.5f
					}))
					.AddComponent(Model.CreateSphere(4f, 4))
					.AddComponent(new ModelRenderer())
					//ring
					.AddChild
					(
						new Entity("ring")
						.AddComponent(marker.ringTransform = new Transform3D()
						{
							LocalPosition = new Vector3(0.0f, 0.0f, 0.0f),
							Rotation = new Vector3(90.0f.ToRadians(), 0f, 0f)
						})
						.AddComponent(new MaterialsMap(marker.spikeMat))
						.AddComponent(Model.CreateTorus(14f, 2, 12))
						.AddComponent(new ModelRenderer())
					)
				)
				//selection
				.AddChild(SelectionRing.Create(node).Owner);
			
			return marker;
		}

		internal NodeMarker(Node n) : base(n) { }

		/////////////////////////////////////////////////////////////////////
		// PROTECTED METHODS
		/////////////////////////////////////////////////////////////////////

		protected override void Initialize()
		{
			base.Initialize();

			APColorPropertyChanged(Entity);
			Entity.OnNodeAccessPointChanged += APColorPropertyChanged;
			Entity.OnNodeNumberChanged += APColorPropertyChanged;
		}

		protected override void Update(TimeSpan gameTime)
		{
			base.Update(gameTime);
			if (!IsOwnerVisible)
				return;

			float secs = (float)gameTime.TotalSeconds;
			float targetAlpha = Entity.Active && (Entity.Selected || CameraTracking || CursorOver) ? 1.0f : 0.5f;
			spikeMat.Alpha = coreMat.Alpha
				= spikeMat.Alpha.Lerp(targetAlpha, secs * FADE_SPEED).Clamp(0.0f, 1.0f);
			spikeMat.DiffuseColor
				= Color.Lerp(spikeMat.DiffuseColor, !Entity.Active ? Color.Black : channelColour, secs * COLOUR_SPEED);
			if (Entity.Active)
			{
				coreMat.DiffuseColor = Color.White.Coserp(channelColour, fader += secs * 2.0f);
				ringTransform.Rotation = new Vector3(
				   ringTransform.Rotation.X,
				   ringTransform.Rotation.Y + ROTATE_SPEED * secs,
				   ringTransform.Rotation.Z
					);
			}
		}

		protected override void ActiveChanged(IUpdateable obj)
		{
			base.ActiveChanged(obj);

			if (!obj.Active)
			{
				ringTransform.Rotation = new Vector3(
				   ringTransform.Rotation.X,
				   0.0f,
				   ringTransform.Rotation.Z
					);
				coreMat.DiffuseColor = Color.White;
			}

			UpdateUI();
		}

		protected override void CursorOverChanged(bool cursorOver)
		{
			base.CursorOverChanged(cursorOver);

			UpdateUI();
		}

		protected override void SelectedChanged(ISelectable obj)
		{
			base.SelectedChanged(obj);

			UpdateUI();
		}

		protected override void UpdateUI()
		{
			UIText.Text = (Entity.Number.HasValue ? Entity.Number.Value.ToString() : "??");
			UISubtext.Text = "ID #" + Entity.ID.ToString("X");
		}

		private void APColorPropertyChanged(Node node)
		{
			channelColour = Entity.AccessPointColor.Wave();
		}
	}
}
