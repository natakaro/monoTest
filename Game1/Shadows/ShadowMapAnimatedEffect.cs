using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game1.Shadows
{
    public class ShadowMapAnimatedEffect
    {
        private readonly Effect _innerEffect;

        private readonly EffectParameter _worldViewProjectionParameter;
        private readonly EffectParameter _bonesParameter;

        public Matrix WorldViewProjection { get; set; }
        public Matrix[] Bones { get; set; }

        public ShadowMapAnimatedEffect(GraphicsDevice graphicsDevice, Effect innerEffect)
        {
            _innerEffect = innerEffect;

            _worldViewProjectionParameter = _innerEffect.Parameters["WorldViewProjection"];
            _bonesParameter = _innerEffect.Parameters["Bones"];
        }

        public void Apply()
        {
            _worldViewProjectionParameter.SetValue(WorldViewProjection);
            _bonesParameter.SetValue(Bones);

            _innerEffect.CurrentTechnique.Passes[0].Apply();
        }
    }
}
