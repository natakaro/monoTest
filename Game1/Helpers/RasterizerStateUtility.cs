using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game1.Helpers
{
    public static class RasterizerStateUtility
    {
        public static readonly RasterizerState CreateShadowMap = new RasterizerState
        {
            CullMode = CullMode.None,
            DepthClipEnable = false
        };
    }
}
