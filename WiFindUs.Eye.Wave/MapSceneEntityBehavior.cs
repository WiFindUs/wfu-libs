using WaveEngine.Framework;
using WaveEngine.Framework.Graphics;

namespace WiFindUs.Eye.Wave
{
	public abstract class MapSceneEntityBehavior : Behavior
	{
		private Transform3D transform;
		private MapScene scene;

		/////////////////////////////////////////////////////////////////////
		// PROPERTIES
		/////////////////////////////////////////////////////////////////////

		public MapScene Scene
		{
			get { return scene; }
		}

		public Transform3D Transform3D
		{
			get { return transform; }
		}

		/////////////////////////////////////////////////////////////////////
		// PROTECTED METHODS
		/////////////////////////////////////////////////////////////////////

		protected override void Initialize()
		{
			base.Initialize();
			scene = Owner.Scene as MapScene;
			transform = Owner.FindComponent<Transform3D>();
		}
	}
}
