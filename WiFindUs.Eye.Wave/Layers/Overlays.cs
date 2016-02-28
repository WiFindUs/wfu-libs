namespace WiFindUs.Eye.Wave.Layers
{
    public class Overlays : Layer
	{
		public Overlays(RenderManager renderManager)
			: base(renderManager)
		{
		}

		protected override void SetDevice()
		{
			renderState.BlendMode = BlendMode.NonPremultiplied;
			renderState.SamplerMode = AddressMode.LinearClamp;
			renderState.CullMode = CullMode.CounterClockWise;
			renderState.FillMode = FillMode.Solid;
			//renderState.DepthMode = DepthMode.Read;
			renderState.MaxAnisotropy = AnisotropyLevel.Aniso1x;
		}

		protected override void RestoreDevice()
		{
		}
	}
}
