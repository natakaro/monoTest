using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Game1
{

    public class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        

        //Camera
        Vector3 camTarget;
        Vector3 camPosition;
        Matrix projectionMatrix;
        Matrix viewMatrix;
        Matrix worldMatrix;
        int mm;
        Vector4 kolor = new Vector4(0,0,1,1);
        //Geometric info
        Model model;

        //Orbit
        bool orbit = false;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferWidth = 1280;
            graphics.PreferredBackBufferHeight = 720;
            
            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {
            base.Initialize();

            //Setup Camera
            camTarget = new Vector3(0f, 0f, 0f);
            camPosition = new Vector3(0f, 0f, -5);
            projectionMatrix = Matrix.CreatePerspectiveFieldOfView(
                               MathHelper.ToRadians(45f), graphics.
                               GraphicsDevice.Viewport.AspectRatio,
                1f, 1000f);
            viewMatrix = Matrix.CreateLookAt(camPosition, camTarget,
                         new Vector3(0f, 1f, 0f));// Y up
            worldMatrix = Matrix.CreateWorld(camTarget, Vector3.
                          Forward, Vector3.Up);

            model = Content.Load<Model>("bla");
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
        }

        protected override void UnloadContent()
        {
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back ==
                ButtonState.Pressed || Keyboard.GetState().IsKeyDown(
                Keys.Escape))
                Exit();

            if (Keyboard.GetState().IsKeyDown(Keys.Left))
            {
                camPosition.X -= 0.1f;
                camTarget.X -= 0.1f;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.Right))
            {
                camPosition.X += 0.1f;
                camTarget.X += 0.1f;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.Up))
            {
                camPosition.Y -= 0.1f;
                camTarget.Y -= 0.1f;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.Down))
            {
                camPosition.Y += 0.1f;
                camTarget.Y += 0.1f;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.OemPlus))
            {
                camPosition.Z += 0.1f;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.OemMinus))
            {
                camPosition.Z -= 0.1f;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.Space))
            {
                orbit = !orbit;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.D1))
            {
                graphics.ToggleFullScreen();
            }
            if (Keyboard.GetState().IsKeyDown(Keys.D7))
            {
                kolor = new Vector4(1, 0, 0, 1);
            }

            if (Mouse.GetState().X > mm) 
            {
                Matrix rotationMatrix = Matrix.CreateRotationY(
                                        MathHelper.ToRadians(1f));
                camPosition = Vector3.Transform(camPosition,
                              rotationMatrix);

                viewMatrix = Matrix.CreateLookAt(camPosition, camTarget,
                         Vector3.Up);
                base.Update(gameTime);
            }
            if (Mouse.GetState().X < mm)
            {
                Matrix rotationMatrix = Matrix.CreateRotationY(
                                        MathHelper.ToRadians(-1f));
                camPosition = Vector3.Transform(camPosition,
                              rotationMatrix);

                viewMatrix = Matrix.CreateLookAt(camPosition, camTarget,
                         Vector3.Up);
                base.Update(gameTime);
            }
            if (orbit)
            {
                Matrix rotationMatrix = Matrix.CreateRotationY(
                                        MathHelper.ToRadians(1f));
                camPosition = Vector3.Transform(camPosition,
                              rotationMatrix);
            }
            viewMatrix = Matrix.CreateLookAt(camPosition, camTarget,
                         Vector3.Up);
            base.Update(gameTime);
            mm = Mouse.GetState().Position.X;
        }

        
        protected override void Draw(GameTime gameTime)
        {
            //GraphicsDevice.Clear(Color.CornflowerBlue);
            GraphicsDevice.Clear(new Color(kolor));
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    //effect.EnableDefaultLighting();
                    effect.AmbientLightColor = new Vector3(1f, 0, 0);
                    effect.View = viewMatrix;
                    effect.World = worldMatrix;
                    effect.Projection = projectionMatrix;
                }
                mesh.Draw();
            }
            base.Draw(gameTime);
        }
    }
}