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
	public class Wireframes : Layer
	{
		public Wireframes(RenderManager renderManager)
			: base(renderManager)
		{
		}

		protected override void SetDevice()
		{
			renderState.BlendMode = BlendMode.NonPremultiplied;
			renderState.SamplerMode = AddressMode.LinearClamp;
			renderState.CullMode = CullMode.None;
			renderState.FillMode = FillMode.Wireframe;
			//renderState.DepthMode = DepthMode.Read;
			renderState.MaxAnisotropy = AnisotropyLevel.Aniso1x;
			//renderState.DepthBias = DepthBias.Negative;
		}

		protected override void RestoreDevice()
		{
		}
	}
}
