using Game1.Helpers;
using Game1.Shadows;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game1.Lights
{
    public class LightManager
    {
        Game1 game;
        RenderTarget2D lightTarget;
        Model pointLightGeometry;
        Effect pointLightEffect;

        #region Point Lights

        List<PointLight> pointLights;

        public List<PointLight> PointLights
        {
            get { return pointLights; }
        }

        public void AddLight(PointLight Light)
        {
            pointLights.Add(Light);
        }

        public void RemoveLight(PointLight Light)
        {
            pointLights.Remove(Light);
        }
        #endregion

        public LightManager(Game1 game, RenderTarget2D lightTarget, ContentManager content)
        {
            this.game = game;
            this.lightTarget = lightTarget;

            //Initialize Point Lights
            pointLights = new List<PointLight>();

            pointLightGeometry = content.Load<Model>("Models/sphere");
            pointLightEffect = content.Load<Effect>("Effects/PointLight");
        }

        public void DrawPointLights()
        {
            foreach (PointLight light in pointLights)
            {
                light.Draw(game, game.camera, pointLightEffect, game.colorTarget, game.normalTarget, game.depthTarget, pointLightGeometry);
            }
        }

        public void Draw()
        {
            game.GraphicsDevice.BlendState = BlendState.AlphaBlend;
            game.GraphicsDevice.DepthStencilState = DepthStencilState.None;

            DrawPointLights();

            game.GraphicsDevice.BlendState = BlendState.Opaque;
            game.GraphicsDevice.DepthStencilState = DepthStencilState.None;
            game.GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
            game.GraphicsDevice.SetRenderTarget(null);
        }
    }
}
