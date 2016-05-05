using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game1.Helpers
{
    public static class SamplerStateUtility
    {
        public static readonly SamplerState ShadowMap = new SamplerState
        {
            AddressU = TextureAddressMode.Clamp,
            AddressV = TextureAddressMode.Clamp,
            AddressW = TextureAddressMode.Clamp,
            Filter = TextureFilter.Linear,
            ComparisonFunction = CompareFunction.LessEqual
        };
    }
}
