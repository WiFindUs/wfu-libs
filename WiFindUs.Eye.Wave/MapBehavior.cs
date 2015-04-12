using WaveEngine.Components.Graphics3D;
using WaveEngine.Framework;
using WaveEngine.Framework.Graphics;

namespace WiFindUs.Eye.Wave
{
	public abstract class MapBehavior : Behavior
	{
		protected const float SPEED_SCALE	= 1f;
		protected const float FADE_SPEED	= 10f * SPEED_SCALE;
		protected const float SCALE_SPEED	= 15f * SPEED_SCALE;
		protected const float MOVE_SPEED	= 5f * SPEED_SCALE;
		protected const float ROTATE_SPEED	= 5f * SPEED_SCALE;
		protected const float COLOUR_SPEED	= FADE_SPEED * 0.15f;
		protected const float CAMERA_SPEED	= MOVE_SPEED * 3.0f;

		private Transform3D transform;
		private MapScene scene;

		/////////////////////////////////////////////////////////////////////
		// PROPERTIES
		/////////////////////////////////////////////////////////////////////

		public MapScene MapScene
		{
			get { return scene; }
		}

		public Transform3D Transform3D
		{
			get { return transform; }
			set { if (transform == null) transform = value; }
		}

		public bool IsOwnerVisible
		{
			get { return Owner.IsVisible; }
			set
			{
				if (value == Owner.IsVisible)
					return;
				Owner.IsVisible = value;
			}
		}

		public bool IsOwnerActive
		{
			get { return Owner.IsActive; }
			set
			{
				if (value == Owner.IsActive)
					return;
				Owner.IsActive = value;
			}
		}

		/////////////////////////////////////////////////////////////////////
		// PROTECTED METHODS
		/////////////////////////////////////////////////////////////////////

		protected override void Initialize()
		{
			base.Initialize();
			scene = Owner.Scene as MapScene;
			Transform3D t3d = Owner.FindComponent<Transform3D>();
			if (t3d != transform)
				transform = t3d;
		}
	}
}
