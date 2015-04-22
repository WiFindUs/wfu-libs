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
		private BasicMaterial spikeMat, ringMat, coreMat;
		private float fader = 0.0f;
		private Color colour = new Color(0, 175, 255);
		private StackPanel uiPanel;
		private TextBlock text, subtext;

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
						.AddComponent(new MaterialsMap(marker.ringMat = new BasicMaterial(MapScene.WhiteTexture)
						{
							LayerType = typeof(NonPremultipliedAlpha),
							LightingEnabled = true,
							AmbientLightColor = Color.White * 0.75f,
							Alpha = 0.5f
						}))
						.AddComponent(Model.CreateTorus(14f, 2, 12))
						.AddComponent(new ModelRenderer())
					)
				)
				//selection
				.AddChild(SelectionRing.Create(node).Owner);

			//ui
			marker.UIEntity = (marker.uiPanel = new StackPanel()
			{
				Orientation = Orientation.Vertical,
				BackgroundColor = Themes.Theme.Current.Background.Dark.Colour.Wave(200),
				IsBorder = false,
				Opacity = 0.0f,
				Width = 100.0f,
				Height = 40.0f,
			}).Entity;
			marker.uiPanel.Add(marker.text = new TextBlock()
			{
				Text = "This is text on line 1",
				Foreground = Themes.Theme.Current.Foreground.Light.Colour.Wave(),
				Width = marker.uiPanel.Width,
				Height = 17.0f,
				TextAlignment = TextAlignment.Center,
				TextWrapping = false,
				IsBorder = false
			});
			marker.uiPanel.Add(marker.subtext = new TextBlock()
			{
				Text = "this is some sub text",
				Foreground = Themes.Theme.Current.Foreground.Light.Colour.Wave(),
				Width = marker.uiPanel.Width * (1.0f / SUBTEXT_SCALE),
				Height = 15.0f * (1.0f / SUBTEXT_SCALE),
				TextAlignment = TextAlignment.Center,
				TextWrapping = false,
				IsBorder = false
			});
			marker.UIEntity.FindComponent<Transform2D>().LocalScale = new Vector2(UI_SCALE);
			marker.subtext.Entity.FindComponent<Transform2D>().LocalScale = new Vector2(SUBTEXT_SCALE);
			return marker;
		}

		internal NodeMarker(Node n) : base(n) { }

		/////////////////////////////////////////////////////////////////////
		// PROTECTED METHODS
		/////////////////////////////////////////////////////////////////////

		protected override void Update(TimeSpan gameTime)
		{
			base.Update(gameTime);
			if (!Owner.IsVisible)
				return;

			float secs = (float)gameTime.TotalSeconds;
			float targetAlpha = Entity.Active && (Entity.Selected || CameraTracking || CursorOver) ? 1.0f : 0.5f;
			spikeMat.Alpha = coreMat.Alpha = ringMat.Alpha =
				spikeMat.Alpha.Lerp(targetAlpha, secs * FADE_SPEED).Clamp(0.0f, 1.0f);
			spikeMat.DiffuseColor = ringMat.DiffuseColor
				= Color.Lerp(spikeMat.DiffuseColor, !Entity.Active ? Color.Black : colour, secs * COLOUR_SPEED);
			if (Entity.Active)
			{
				coreMat.DiffuseColor = Color.White.Coserp(colour, fader += secs * 2.0f);
				ringTransform.Rotation = new Vector3(
				   ringTransform.Rotation.X,
				   ringTransform.Rotation.Y + ROTATE_SPEED * secs,
				   ringTransform.Rotation.Z
					);
			}

			//ui
			float uiAlpha = Entity.Selected || CursorOver ? 1.0f : (EntityWaiting ? 0.5f : 0.0f);
			if (UITransform != null && uiAlpha > 0.0f)
				UITransform.Position
					= ScreenPosition.Add(uiPanel.Width * UI_SCALE * -0.5f, 3.0f);
			uiPanel.Opacity = uiPanel.Opacity.Lerp(uiAlpha, secs * FADE_SPEED).Clamp(0.0f,1.0f);
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

		/////////////////////////////////////////////////////////////////////
		// PRIVATE METHODS
		/////////////////////////////////////////////////////////////////////

		private void UpdateUI()
		{
			text.Text = "#" + (entity.Number.HasValue ? entity.Number.Value.ToString() : "??");
			subtext.Text = "ID: " + entity.ID.ToString("X");
		}
	}
}
