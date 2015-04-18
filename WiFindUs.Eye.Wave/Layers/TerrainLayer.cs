using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WaveEngine.Common.Graphics;
using WaveEngine.Framework.Graphics;
using WaveEngine.Framework.Managers;

namespace WiFindUs.Eye.Wave.Layers
{
	public class TerrainLayer : Layer
	{
		public TerrainLayer(RenderManager renderManager)
			: base(renderManager)
		{
		}

		protected override void SetDevice()
		{
			renderState.BlendMode = BlendMode.NonPremultiplied;
			renderState.SamplerMode = AddressMode.AnisotropicClamp;
			renderState.CullMode = CullMode.CounterClockWise;
			renderState.FillMode = FillMode.Solid;
			renderState.DepthMode = DepthMode.Read;
			renderState.DepthBias = DepthBias.Positive;
			renderState.MaxAnisotropy = AnisotropyLevel.Aniso2x;
		}

		protected override void RestoreDevice()
		{
		}
	}
}
