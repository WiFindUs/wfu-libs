using WaveEngine.Framework.Physics3D;

namespace WiFindUs.Eye.Wave.Markers
{
	public abstract class Marker : MapSceneEntityBehavior
	{
		protected const float MAX_SPIN_RATE = 5.0f;

		/////////////////////////////////////////////////////////////////////
		// PROPERTIES
		/////////////////////////////////////////////////////////////////////

		public abstract BoxCollider BoxCollider { get; }

		public abstract bool Selected { get; set; }
	}
}
