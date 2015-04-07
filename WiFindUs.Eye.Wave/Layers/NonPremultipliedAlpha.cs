﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WaveEngine.Common.Graphics;
using WaveEngine.Framework.Graphics;
using WaveEngine.Framework.Managers;

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
			renderState.DepthMode = DepthMode.Write;
			renderState.MaxAnisotropy = AnisotropyLevel.Aniso1x;
		}

		protected override void RestoreDevice()
		{
		}
	}
}