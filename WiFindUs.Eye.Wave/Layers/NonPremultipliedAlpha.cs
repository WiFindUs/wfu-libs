namespace WiFindUs.Eye.Wave.Layers
{
    public class NonPremultipliedAlpha : Layer
	{
		public NonPremultipliedAlpha(RenderManager renderManager)
			: base(renderManager)
		{
		}

		protected override void SetDevice()
		{
			renderState.BlendMode = BlendMode.NonPremultiplied;
			renderState.SamplerMode = AddressMode.AnisotropicClamp;
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
