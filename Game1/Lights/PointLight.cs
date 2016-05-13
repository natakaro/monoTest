using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game1.Lights
{
    public class PointLight
    {
        Vector3 position;
        Color color;
        float radius;
        float intensity;

        public Vector3 Position
        {
            get { return position; }
            set { position = value; }
        }
        
        public PointLight(Vector3 position, Color color, float radius, float intensity)
        {
            this.position = position;
            this.color = color;
            this.radius = radius;
            this.intensity = intensity;
        }

        public void Draw(Game1 game, Camera camera, Effect pointLightEffect, RenderTarget2D colorTarget, RenderTarget2D normalTarget, RenderTarget2D depthTarget, Model pointLightGeometry)
        {
            //set the G-Buffer parameters
            pointLightEffect.Parameters["colorMap"].SetValue(colorTarget);
            pointLightEffect.Parameters["normalMap"].SetValue(normalTarget);
            pointLightEffect.Parameters["depthMap"].SetValue(depthTarget);

            //compute the light world matrix
            //scale according to light radius, and translate it to light position
            Matrix sphereWorldMatrix = Matrix.CreateScale(radius) * Matrix.CreateTranslation(position);
            pointLightEffect.Parameters["World"].SetValue(sphereWorldMatrix);
            pointLightEffect.Parameters["View"].SetValue(camera.ViewMatrix);
            pointLightEffect.Parameters["Projection"].SetValue(camera.ProjectionMatrix);
            //light position
            pointLightEffect.Parameters["lightPosition"].SetValue(position);

            //set the color, radius and Intensity
            pointLightEffect.Parameters["Color"].SetValue(color.ToVector3());
            pointLightEffect.Parameters["lightRadius"].SetValue(radius);
            pointLightEffect.Parameters["lightIntensity"].SetValue(intensity);

            //parameters for specular computations
            pointLightEffect.Parameters["cameraPosition"].SetValue(position);
            pointLightEffect.Parameters["InvertViewProjection"].SetValue(Matrix.Invert(camera.ViewProjectionMatrix));

            //calculate the distance between the camera and light center
            float cameraToCenter = Vector3.Distance(position, position);
            //if we are inside the light volume, draw the sphere's inside face
            if (cameraToCenter < radius)
                game.GraphicsDevice.RasterizerState = RasterizerState.CullClockwise;
            else
                game.GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;

            game.GraphicsDevice.DepthStencilState = DepthStencilState.None;

            pointLightEffect.Techniques[0].Passes[0].Apply();
            foreach (ModelMesh mesh in pointLightGeometry.Meshes)
            {
                foreach (ModelMeshPart meshPart in mesh.MeshParts)
                {
                    game.GraphicsDevice.Indices = meshPart.IndexBuffer;
                    game.GraphicsDevice.SetVertexBuffer(meshPart.VertexBuffer);

                    game.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, meshPart.PrimitiveCount);
                }
            }

            game.GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
            game.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
        }
    }
}
