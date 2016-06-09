using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game1.Helpers
{
    /// <summary>
    /// Used for textures that store intermediate results of
    /// passes during post-processing
    /// </summary>
    public class IntermediateTexture
    {
        public RenderTarget2D RenderTarget;
        public bool InUse;
    }
}
