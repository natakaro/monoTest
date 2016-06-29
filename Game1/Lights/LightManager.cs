using Game1.Helpers;
using Game1.Screens;
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
        Game game;
        ScreenManager screenManager;
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

        public LightManager(Game game, ScreenManager screenManager, RenderTarget2D lightTarget, ContentManager content)
        {
            this.game = game;
            this.screenManager = screenManager;
            this.lightTarget = lightTarget;

            //Initialize Point Lights
            pointLights = new List<PointLight>();

            pointLightGeometry = content.Load<Model>("Models/pointLightGeometry");
            pointLightEffect = content.Load<Effect>("Effects/PointLight");
        }

        public void DrawPointLights(Camera camera, RenderTarget2D colorTarget, RenderTarget2D normalTarget, RenderTarget2D depthTarget)
        {
            foreach (PointLight light in pointLights)
            {
                if (GameplayScreen.camera.Frustum.Contains(light.BoundingSphere) == ContainmentType.Disjoint)
                    continue;
                light.Draw(game, camera, pointLightEffect, colorTarget, normalTarget, depthTarget, pointLightGeometry);
            }
        }

        public void Draw(Camera camera, RenderTarget2D colorTarget, RenderTarget2D normalTarget, RenderTarget2D depthTarget)
        {
            game.GraphicsDevice.BlendState = BlendState.AlphaBlend;
            game.GraphicsDevice.DepthStencilState = DepthStencilState.None;

            DrawPointLights(camera, colorTarget, normalTarget, depthTarget);

            game.GraphicsDevice.BlendState = BlendState.Opaque;
            game.GraphicsDevice.DepthStencilState = DepthStencilState.None;
            game.GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
            game.GraphicsDevice.SetRenderTarget(null);
        }
    }
}
