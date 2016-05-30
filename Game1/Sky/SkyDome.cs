/*
 * Skydome Component
 * 
 * Alex Urbano Álvarez
 * XNA Community Coordinator
 * 
 * goefuika@gmail.com
 * 
 * http://elgoe.blogspot.com
 * http://www.codeplex.com/XNACommunity
 */

using Game1.Helpers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game1.Sky
{
    public class SkyDome
    {
        #region Properties

        private float fTheta;
        private float fPhi;

        private float previousTheta, previousPhi;

        private bool realTime;

        Camera camera;
        Game game;

        Texture2D mieTex, rayleighTex;
        RenderTarget2D mieRT, rayleighRT;

        Texture2D moonTex, glowTex, starsTex;

        Texture2D permTex;

        Effect scatterEffect, texturedEffect, noiseEffect;

        QuadRenderComponent quad;

        SkyDomeParameters parameters;

        VertexPositionTexture[] domeVerts, quadVerts, planeVerts;
        short[] ib, quadIb, planeIb;

        int DomeN;
        int DVSize;
        int DISize;

        int PVSize;
        int PISize;

        Vector4 sunColor;

        private float inverseCloudVelocity;
        private float cloudCover;
        private float cloudSharpness;
        private float numTiles;

        #endregion

        #region Gets/Sets
        /// <summary>
        /// Gets/Sets Theta value
        /// </summary>
        public float Theta { get { return fTheta; } set { fTheta = value; } }

        /// <summary>
        /// Gets/Sets Phi value
        /// </summary>
        public float Phi { get { return fPhi; } set { fPhi = value; } }

        /// <summary>
        /// Gets/Sets actual time computation
        /// </summary>
        public bool RealTime
        {
            get { return realTime; }
            set { realTime = value; }
        }

        /// <summary>
        /// Gets/Sets the SkyDome parameters
        /// </summary>
        public SkyDomeParameters Parameters { get { return parameters; } set { parameters = value; } }

        /// <summary>
        /// Gets the Sun color
        /// </summary>
        public Vector4 SunColor { get { return sunColor; } }

        /// <summary>
        /// Gets/Sets InverseCloudVelocity value
        /// </summary>
        public float InverseCloudVelocity { get { return inverseCloudVelocity; } set { inverseCloudVelocity = value; } }

        /// <summary>
        /// Gets/Sets CloudCover value
        /// </summary>
        public float CloudCover { get { return cloudCover; } set { cloudCover = value; } }

        /// <summary>
        /// Gets/Sets CloudSharpness value
        /// </summary>
        public float CloudSharpness { get { return cloudSharpness; } set { cloudSharpness = value; } }

        /// <summary>
        /// Gets/Sets CloudSharpness value
        /// </summary>
        public float NumTiles { get { return numTiles; } set { numTiles = value; } }

        #endregion

        #region Contructor

        public SkyDome(Game game, ref Camera camera)
        {
            this.game = game;
            this.camera = camera;

            realTime = false;

            parameters = new SkyDomeParameters();

            quad = new QuadRenderComponent(game);
            game.Components.Add(quad);

            fTheta = 0.0f;
            fPhi = 0.0f;

            DomeN = 32;

            GeneratePermTex();
        }

        #endregion

        #region Initialize
        /// <summary>
        /// Allows the game component to perform any initialization it needs to before starting
        /// to run.  This is where it can query for any required services and load content.
        /// </summary>
        public void Initialize()
        {
            // You can use SurfaceFormat.Color to increase performance / reduce quality
            mieRT = new RenderTarget2D(game.GraphicsDevice, 128, 64, true,
                SurfaceFormat.HalfVector4, DepthFormat.None);
            rayleighRT = new RenderTarget2D(game.GraphicsDevice, 128, 64, true,
                SurfaceFormat.HalfVector4, DepthFormat.None);

            // Clouds constants
            inverseCloudVelocity = 16.0f;
            CloudCover = 0.1f;
            CloudSharpness = 0.5f;
            numTiles = 16.0f;
        }
        #endregion

        #region Load

        public void LoadContent()
        {

            scatterEffect = game.Content.Load<Effect>("Effects/scatter");
            texturedEffect = game.Content.Load<Effect>("Effects/Textured");
            noiseEffect = game.Content.Load<Effect>("Effects/SNoise");

            //oldDepthBuffer = this.GraphicsDevice.DepthStencilBuffer;

            //this.newDepthBuffer = new DepthStencilBuffer(game.GraphicsDevice,
            //    game.GraphicsDevice.PresentationParameters.BackBufferWidth,
            //    game.GraphicsDevice.PresentationParameters.BackBufferHeight,
            //    game.GraphicsDevice.DepthStencilBuffer.Format, MultiSampleType.None, 0);

            moonTex = game.Content.Load<Texture2D>("Textures/moon");
            glowTex = game.Content.Load<Texture2D>("Textures/moonglow");
            starsTex = game.Content.Load<Texture2D>("Textures/starfield");
            
            GenerateDome();
            GenerateMoon();
            //GeneratePlane();
        }

        #endregion

        #region Update
        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public void Update(GameTime gameTime)
        {
            if (realTime)
            {
                int minutes = DateTime.Now.Hour * 60 + DateTime.Now.Minute;
                fTheta = minutes * (float)(Math.PI) / 12.0f / 60.0f;
            }

            parameters.LightDirection = GetDirection();
            parameters.LightDirection.Normalize();
        }
        #endregion

        #region Draw
        /// <summary>
        /// Draws the component.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public void Draw(GameTime gameTime, Matrix viewMatrix, Vector3 cameraPosition)
        {
            Matrix View = viewMatrix;
            Matrix Projection = camera.ProjectionMatrix;
            Matrix World = Matrix.CreateTranslation(cameraPosition.X,
                cameraPosition.Y,
                cameraPosition.Z);

            if (previousTheta != fTheta || previousPhi != fPhi)
                UpdateMieRayleighTextures();

            sunColor = GetSunColor(-fTheta, 2);

            //game.GraphicsDevice.Clear(Color.CornflowerBlue);

            DepthStencilState prevDepthState = game.GraphicsDevice.DepthStencilState;
            RasterizerState prevRasterizerState = game.GraphicsDevice.RasterizerState;
            game.GraphicsDevice.DepthStencilState = DepthStencilState.None;
            game.GraphicsDevice.RasterizerState = RasterizerState.CullNone;

            scatterEffect.CurrentTechnique = scatterEffect.Techniques["Render"];
            scatterEffect.Parameters["txMie"].SetValue(mieTex);
            scatterEffect.Parameters["txRayleigh"].SetValue(rayleighTex);
            scatterEffect.Parameters["WorldViewProjection"].SetValue(World * View * Projection);
            scatterEffect.Parameters["v3SunDir"].SetValue(new Vector3(-parameters.LightDirection.X,
                -parameters.LightDirection.Y, -parameters.LightDirection.Z));
            scatterEffect.Parameters["NumSamples"].SetValue(parameters.NumSamples);
            scatterEffect.Parameters["fExposure"].SetValue(parameters.Exposure);
            scatterEffect.Parameters["StarsTex"].SetValue(starsTex);
            if (fTheta < Math.PI / 2.0f || fTheta > 3.0f * Math.PI / 2.0f)
                scatterEffect.Parameters["starIntensity"].SetValue((float)Math.Abs(
                    Math.Sin(Theta + (float)Math.PI / 2.0f)));
            else
                scatterEffect.Parameters["starIntensity"].SetValue(0.0f);

            foreach (EffectPass pass in scatterEffect.CurrentTechnique.Passes)
            {
                pass.Apply();

                game.GraphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, domeVerts, 0, DVSize, ib, 0, DISize);
            }

            //DrawGlow(viewMatrix, cameraPosition);
            DrawMoon(viewMatrix, cameraPosition);
            DrawClouds(gameTime, viewMatrix, cameraPosition);

            game.GraphicsDevice.DepthStencilState = prevDepthState;
            game.GraphicsDevice.RasterizerState = prevRasterizerState;

            previousTheta = fTheta;
            previousPhi = fPhi;

        }

        #region DrawMoon

        private void DrawMoon(Matrix viewMatrix, Vector3 cameraPosition)
        {
            BlendState prevState = game.GraphicsDevice.BlendState;
            game.GraphicsDevice.BlendState = BlendState.AlphaBlend;

            texturedEffect.CurrentTechnique = texturedEffect.Techniques["Textured"];
            texturedEffect.Parameters["World"].SetValue(
                Matrix.CreateRotationX(Theta + (float)Math.PI / 2.0f) *
                Matrix.CreateRotationY(-Phi + (float)Math.PI / 2.0f) *
                Matrix.CreateTranslation(parameters.LightDirection.X * 15,
                parameters.LightDirection.Y * 15,
                parameters.LightDirection.Z * 15) *
                Matrix.CreateTranslation(cameraPosition.X,
                cameraPosition.Y,
                cameraPosition.Z));
            texturedEffect.Parameters["View"].SetValue(viewMatrix);
            texturedEffect.Parameters["Projection"].SetValue(camera.ProjectionMatrix);
            texturedEffect.Parameters["Texture"].SetValue(moonTex);
            if (fTheta < Math.PI / 2.0f || fTheta > 3.0f * Math.PI / 2.0f)
                texturedEffect.Parameters["alpha"].SetValue((float)Math.Abs(
                    Math.Sin(Theta + (float)Math.PI / 2.0f)));
            else
                texturedEffect.Parameters["alpha"].SetValue(0.0f);
            foreach (EffectPass pass in texturedEffect.CurrentTechnique.Passes)
            {
                pass.Apply();

                game.GraphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, quadVerts, 0, 4, quadIb, 0, 2);
            }

            game.GraphicsDevice.BlendState = prevState;

        }

        #endregion

        #region DrawGlow

        private void DrawGlow(Matrix viewMatrix, Vector3 cameraPosition)
        {
            BlendState prevState = game.GraphicsDevice.BlendState;
            game.GraphicsDevice.BlendState = BlendState.AlphaBlend;

            texturedEffect.CurrentTechnique = texturedEffect.Techniques["Textured"];
            texturedEffect.Parameters["World"].SetValue(
                Matrix.CreateRotationX(Theta + (float)Math.PI / 2.0f) *
                Matrix.CreateRotationY(-Phi + (float)Math.PI / 2.0f) *
                Matrix.CreateTranslation(parameters.LightDirection.X * 5,
                parameters.LightDirection.Y * 5,
                parameters.LightDirection.Z * 5) *
                Matrix.CreateTranslation(cameraPosition.X,
                cameraPosition.Y,
                cameraPosition.Z));
            texturedEffect.Parameters["View"].SetValue(viewMatrix);
            texturedEffect.Parameters["Projection"].SetValue(camera.ProjectionMatrix);
            texturedEffect.Parameters["Texture"].SetValue(glowTex);
            if (fTheta < Math.PI / 2.0f || fTheta > 3.0f * Math.PI / 2.0f)
                texturedEffect.Parameters["alpha"].SetValue((float)Math.Abs(
                    Math.Sin(Theta + (float)Math.PI / 2.0f)));
            else
                texturedEffect.Parameters["alpha"].SetValue(0.0f);
            foreach (EffectPass pass in texturedEffect.CurrentTechnique.Passes)
            {
                pass.Apply();

                game.GraphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, quadVerts, 0, 4, quadIb, 0, 2);
            }

            game.GraphicsDevice.BlendState = prevState;

        }

        #endregion

        #region DrawClouds

        private void DrawClouds(GameTime gameTime, Matrix viewMatrix, Vector3 cameraPosition)
        {
            BlendState prevState = game.GraphicsDevice.BlendState;
            game.GraphicsDevice.BlendState = BlendState.AlphaBlend;

            noiseEffect.CurrentTechnique = noiseEffect.Techniques["Noise"];
            noiseEffect.Parameters["World"].SetValue(Matrix.CreateScale(2000.0f) *
                Matrix.CreateTranslation(new Vector3(0, 0, -100)) *
                Matrix.CreateRotationX((float)Math.PI / 2.0f) *
                Matrix.CreateTranslation(cameraPosition.X,
                cameraPosition.Y,
                cameraPosition.Z)
                );
            noiseEffect.Parameters["View"].SetValue(viewMatrix);
            noiseEffect.Parameters["Projection"].SetValue(camera.ProjectionMatrix);
            noiseEffect.Parameters["permTexture"].SetValue(permTex);
            noiseEffect.Parameters["time"].SetValue((float)gameTime.TotalGameTime.TotalSeconds / inverseCloudVelocity);
            noiseEffect.Parameters["SunColor"].SetValue(sunColor);
            noiseEffect.Parameters["v3SunDir"].SetValue(new Vector3(-parameters.LightDirection.X,
                -parameters.LightDirection.Y, -parameters.LightDirection.Z));
            noiseEffect.Parameters["cameraPosition"].SetValue(cameraPosition);
            noiseEffect.Parameters["numTiles"].SetValue(numTiles);
            noiseEffect.Parameters["CloudCover"].SetValue(cloudCover);
            noiseEffect.Parameters["CloudSharpness"].SetValue(cloudSharpness);

            foreach (EffectPass pass in noiseEffect.CurrentTechnique.Passes)
            {
                pass.Apply();

                game.GraphicsDevice.DrawUserIndexedPrimitives<VertexPositionTexture>
                    (PrimitiveType.TriangleList, quadVerts, 0, 4, quadIb, 0, 2);

                //game.GraphicsDevice.DrawUserIndexedPrimitives<VertexPositionTexture>
                //        (PrimitiveType.TriangleList, planeVerts, 0, PVSize, planeIb, 0, PISize);
            }

            game.GraphicsDevice.BlendState = prevState;

        }

        #endregion

        #endregion

        #region Private Methods

        #region Get Light Direction
        Vector4 GetDirection()
        {

            float y = (float)Math.Cos(fTheta);
            float x = (float)(Math.Sin(fTheta) * Math.Cos(fPhi));
            float z = (float)(Math.Sin(fTheta) * Math.Sin(fPhi));
            float w = 1.0f;

            return new Vector4(x, y, z, w);
        }
        #endregion

        #region UpdateMieRayleighTextures

        void UpdateMieRayleighTextures()
        {
            DepthStencilState prevState = game.GraphicsDevice.DepthStencilState;

            RenderTargetBinding[] prevTargets = game.GraphicsDevice.GetRenderTargets();
            game.GraphicsDevice.SetRenderTargets(rayleighRT, mieRT);
            game.GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            game.GraphicsDevice.Clear(Color.CornflowerBlue);

            scatterEffect.CurrentTechnique = scatterEffect.Techniques["Update"];
            scatterEffect.Parameters["InvWavelength"].SetValue(parameters.InvWaveLengths);
            scatterEffect.Parameters["WavelengthMie"].SetValue(parameters.WaveLengthsMie);
            scatterEffect.Parameters["v3SunDir"].SetValue(new Vector3(-parameters.LightDirection.X,
                -parameters.LightDirection.Y, -parameters.LightDirection.Z));
            EffectPass pass = scatterEffect.CurrentTechnique.Passes[0];
            pass.Apply();
            quad.Render(Vector2.One * -1, Vector2.One);

            game.GraphicsDevice.SetRenderTargets(null);

            mieTex = mieRT;
            rayleighTex = rayleighRT;

            game.GraphicsDevice.DepthStencilState = prevState;
            game.GraphicsDevice.SetRenderTargets(prevTargets);

            //mieTex.Save("mimie.dds", ImageFileFormat.Dds);
            //rayleighTex.Save("mirayleigh.dds", ImageFileFormat.Dds);
        }

        #endregion

        #region GenerateDome

        private void GenerateDome()
        {
            int Latitude = DomeN / 2;
            int Longitude = DomeN;
            DVSize = Longitude * Latitude;
            DISize = (Longitude - 1) * (Latitude - 1) * 2;
            DVSize *= 2;
            DISize *= 2;

            domeVerts = new VertexPositionTexture[DVSize];

            // Fill Vertex Buffer
            int DomeIndex = 0;
            for (int i = 0; i < Longitude; i++)
            {
                double MoveXZ = 100.0f * (i / ((float)Longitude - 1.0f)) * MathHelper.Pi / 180.0;

                for (int j = 0; j < Latitude; j++)
                {
                    double MoveY = MathHelper.Pi * j / (Latitude - 1);

                    domeVerts[DomeIndex] = new VertexPositionTexture();
                    domeVerts[DomeIndex].Position.X = (float)(Math.Sin(MoveXZ) * Math.Cos(MoveY));
                    domeVerts[DomeIndex].Position.Y = (float)Math.Cos(MoveXZ);
                    domeVerts[DomeIndex].Position.Z = (float)(Math.Sin(MoveXZ) * Math.Sin(MoveY));

                    domeVerts[DomeIndex].Position *= 10.0f;

                    domeVerts[DomeIndex].TextureCoordinate.X = 0.5f / (float)Longitude + i / (float)Longitude;
                    domeVerts[DomeIndex].TextureCoordinate.Y = 0.5f / (float)Latitude + j / (float)Latitude;

                    DomeIndex++;
                }
            }
            for (int i = 0; i < Longitude; i++)
            {
                double MoveXZ = 100.0 * (i / (float)(Longitude - 1)) * MathHelper.Pi / 180.0;

                for (int j = 0; j < Latitude; j++)
                {
                    double MoveY = (MathHelper.Pi * 2.0) - (MathHelper.Pi * j / (Latitude - 1));

                    domeVerts[DomeIndex] = new VertexPositionTexture();
                    domeVerts[DomeIndex].Position.X = (float)(Math.Sin(MoveXZ) * Math.Cos(MoveY));
                    domeVerts[DomeIndex].Position.Y = (float)Math.Cos(MoveXZ);
                    domeVerts[DomeIndex].Position.Z = (float)(Math.Sin(MoveXZ) * Math.Sin(MoveY));

                    domeVerts[DomeIndex].Position *= 10.0f;

                    domeVerts[DomeIndex].TextureCoordinate.X = 0.5f / (float)Longitude + i / (float)Longitude;
                    domeVerts[DomeIndex].TextureCoordinate.Y = 0.5f / (float)Latitude + j / (float)Latitude;

                    DomeIndex++;
                }
            }

            // Fill index buffer
            ib = new short[DISize * 3];
            int index = 0;
            for (short i = 0; i < Longitude - 1; i++)
            {
                for (short j = 0; j < Latitude - 1; j++)
                {
                    ib[index++] = (short)(i * Latitude + j);
                    ib[index++] = (short)((i + 1) * Latitude + j);
                    ib[index++] = (short)((i + 1) * Latitude + j + 1);

                    ib[index++] = (short)((i + 1) * Latitude + j + 1);
                    ib[index++] = (short)(i * Latitude + j + 1);
                    ib[index++] = (short)(i * Latitude + j);
                }
            }
            short Offset = (short)(Latitude * Longitude);
            for (short i = 0; i < Longitude - 1; i++)
            {
                for (short j = 0; j < Latitude - 1; j++)
                {
                    ib[index++] = (short)(Offset + i * Latitude + j);
                    ib[index++] = (short)(Offset + (i + 1) * Latitude + j + 1);
                    ib[index++] = (short)(Offset + (i + 1) * Latitude + j);

                    ib[index++] = (short)(Offset + i * Latitude + j + 1);
                    ib[index++] = (short)(Offset + (i + 1) * Latitude + j + 1);
                    ib[index++] = (short)(Offset + i * Latitude + j);
                }
            }
        }

        #endregion

        #region GeneratePlane
        private void GeneratePlane()
        {
            int planeResolution = 20;
            float planeWidth = 10.0f;
            float planeTop = 0.5f;
            float planeBottom = -0.5f;
            int textureRepeat = 1;

            float quadSize = planeWidth / (float)planeResolution;
            float radius = planeWidth / 2.0f;
            float constant = (planeTop - planeBottom) / (radius * radius);
            float textureDelta = (float)textureRepeat / (float)planeResolution;

            PVSize = planeResolution * planeResolution;
            PISize = (planeResolution - 1) * (planeResolution - 1) * 2;
            PVSize *= 2;
            PISize *= 2;

            planeVerts = new VertexPositionTexture[PVSize];

            // Fill Vertex Buffer
            int PlaneIndex = 0;
            for (int i = 0; i < planeResolution; i++)
            {
                for (int j = 0; j < planeResolution; j++)
                {
                    planeVerts[PlaneIndex] = new VertexPositionTexture();

                    float positionX = (-0.5f * planeWidth) + (i * quadSize);
                    float positionZ = (-0.5f * planeWidth) + (j * quadSize);
                    float positionY = planeTop - (constant * ((positionX * positionX) + (positionZ * positionZ)));
                    planeVerts[PlaneIndex].Position.X = positionX;
                    planeVerts[PlaneIndex].Position.Y = positionY;
                    planeVerts[PlaneIndex].Position.Z = positionZ;

                    planeVerts[PlaneIndex].TextureCoordinate.X = i * textureDelta;
                    planeVerts[PlaneIndex].TextureCoordinate.Y = j * textureDelta;

                    PlaneIndex++;
                }
            }

            // Fill index buffer
            planeIb = new short[PISize * 3];
            int index = 0;
            for (short i = 0; i < planeResolution - 1; i++)
            {
                for (short j = 0; j < planeResolution - 1; j++)
                {
                    planeIb[index++] = (short)(i * planeResolution + j);
                    planeIb[index++] = (short)((i + 1) * planeResolution + j);
                    planeIb[index++] = (short)((i + 1) * planeResolution + j + 1);

                    planeIb[index++] = (short)((i + 1) * planeResolution + j + 1);
                    planeIb[index++] = (short)(i * planeResolution + j + 1);
                    planeIb[index++] = (short)(i * planeResolution + j);
                }
            }
            short Offset = (short)(planeResolution * planeResolution);
            for (short i = 0; i < planeResolution - 1; i++)
            {
                for (short j = 0; j < planeResolution - 1; j++)
                {
                    planeIb[index++] = (short)(Offset + i * planeResolution + j);
                    planeIb[index++] = (short)(Offset + (i + 1) * planeResolution + j + 1);
                    planeIb[index++] = (short)(Offset + (i + 1) * planeResolution + j);

                    planeIb[index++] = (short)(Offset + i * planeResolution + j + 1);
                    planeIb[index++] = (short)(Offset + (i + 1) * planeResolution + j + 1);
                    planeIb[index++] = (short)(Offset + i * planeResolution + j);
                }
            }
        }
        #endregion

        #region GenerateMoon

        private void GenerateMoon()
        {
            quadVerts = new VertexPositionTexture[]
                        {
                            new VertexPositionTexture(
                                new Vector3(1,-1,0),
                                new Vector2(1,1)),
                            new VertexPositionTexture(
                                new Vector3(-1,-1,0),
                                new Vector2(0,1)),
                            new VertexPositionTexture(
                                new Vector3(-1,1,0),
                                new Vector2(0,0)),
                            new VertexPositionTexture(
                                new Vector3(1,1,0),
                                new Vector2(1,0))
                        };

            quadIb = new short[] { 0, 1, 2, 2, 3, 0 };
        }

        #endregion

        #region GetSunColor

        Vector4 GetSunColor(float fTheta, int nTurbidity)
        {
            float fBeta = 0.046f * nTurbidity - 0.045f;
            float fTauR, fTauA;
            float[] fTau = new float[3];

            float coseno = (float)Math.Cos((double)fTheta + Math.PI);
            double factor = (double)fTheta / Math.PI * 180.0;
            double jarl = Math.Pow(93.885 - factor, -1.253);
            float potencia = (float)jarl;
            float m = 1.0f / (coseno + 0.15f * potencia);

            int i;
            float[] fLambda = new float[3];
            fLambda[0] = parameters.WaveLengths.X;
            fLambda[1] = parameters.WaveLengths.Y;
            fLambda[2] = parameters.WaveLengths.Z;


            for (i = 0; i < 3; i++)
            {
                potencia = (float)Math.Pow((double)fLambda[i], 4.0);
                fTauR = (float)Math.Exp((double)(-m * 0.008735f * potencia));

                const float fAlpha = 1.3f;
                potencia = (float)Math.Pow((double)fLambda[i], (double)-fAlpha);
                if (m < 0.0f)
                    fTau[i] = 0.0f;
                else
                {
                    fTauA = (float)Math.Exp((double)(-m * fBeta * potencia));
                    fTau[i] = fTauR * fTauA;
                }

            }

            Vector4 vAttenuation = new Vector4(fTau[0], fTau[1], fTau[2], 1.0f);
            return vAttenuation;
        }

        #endregion

        #region GeneratePermTex

        private void GeneratePermTex()
        {
            int[] perm = { 151,160,137,91,90,15,
            131,13,201,95,96,53,194,233,7,225,140,36,103,30,69,142,8,99,37,240,21,10,23,
            190, 6,148,247,120,234,75,0,26,197,62,94,252,219,203,117,35,11,32,57,177,33,
            88,237,149,56,87,174,20,125,136,171,168, 68,175,74,165,71,134,139,48,27,166,
            77,146,158,231,83,111,229,122,60,211,133,230,220,105,92,41,55,46,245,40,244,
            102,143,54, 65,25,63,161, 1,216,80,73,209,76,132,187,208, 89,18,169,200,196,
            135,130,116,188,159,86,164,100,109,198,173,186, 3,64,52,217,226,250,124,123,
            5,202,38,147,118,126,255,82,85,212,207,206,59,227,47,16,58,17,182,189,28,42,
            223,183,170,213,119,248,152, 2,44,154,163, 70,221,153,101,155,167, 43,172,9,
            129,22,39,253, 19,98,108,110,79,113,224,232,178,185, 112,104,218,246,97,228,
            251,34,242,193,238,210,144,12,191,179,162,241, 81,51,145,235,249,14,239,107,
            49,192,214, 31,181,199,106,157,184, 84,204,176,115,121,50,45,127, 4,150,254,
            138,236,205,93,222,114,67,29,24,72,243,141,128,195,78,66,215,61,156,180
            };

            int[] gradValues = { 1,1,0,
                -1,1,0, 1,-1,0,
                -1,-1,0, 1,0,1,
                -1,0,1, 1,0,-1,
                -1,0,-1, 0,1,1,
                0,-1,1, 0,1,-1,
                0,-1,-1, 1,1,0,
                0,-1,1, -1,1,0,
                0,-1,-1
            };

            permTex = new Texture2D(game.GraphicsDevice, 256, 256, false,
                SurfaceFormat.Color);

            byte[] pixels;
            pixels = new byte[256 * 256 * 4];
            for (int i = 0; i < 256; i++)
            {
                for (int j = 0; j < 256; j++)
                {
                    int offset = (i * 256 + j) * 4;
                    byte value = (byte)perm[(j + perm[i]) & 0xFF];
                    pixels[offset + 1] = (byte)(gradValues[value & 0x0F] * 64 + 64);
                    pixels[offset + 2] = (byte)(gradValues[value & 0x0F + 1] * 64 + 64);
                    pixels[offset + 3] = (byte)(gradValues[value & 0x0F + 2] * 64 + 64);
                    pixels[offset] = value;
                }
            }

            permTex.SetData<byte>(pixels);
            //permTex.SaveAsPng(new System.IO.FileStream("../Tex.png", System.IO.FileMode.Create), 256, 256);
        }

        #endregion

        #endregion

        #region Public Methods

        public void ApplyChanges()
        {
            this.UpdateMieRayleighTextures();
        }

        #endregion
    }
}
