using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game1.Helpers
{
    public class ColorGradingHelper
    {
        public static Texture3D LoadColorGrading(GraphicsDevice graphics, ContentManager content, string assetName)
        {
            Texture2D baseTexture = content.Load<Texture2D>(assetName);
            int precision = baseTexture.Height;
            Color[] grading = new Color[(precision * precision) * precision];
            baseTexture.GetData<Color>(grading);
            baseTexture.Dispose();

            Texture3D colorGrading = new Texture3D(graphics, precision, precision, precision, false, SurfaceFormat.Color);
            colorGrading.SetData<Color>(grading);
            return colorGrading;
        }
    }
}
