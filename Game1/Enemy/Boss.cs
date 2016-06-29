using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Game1.Helpers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using AnimationAux;
using static Game1.Helpers.HexCoordinates;
using Game1.Items;
using Game1.Screens;

namespace Game1
{
    public class Boss : Enemy
    {

        protected AnimatedModel animatedModel = null;
        protected AnimatedModel walk = null;
        AnimationPlayer player = null;
        int counter = 1;
        bool dupy = true;
        Camera cam = null;
        bool done = false;
        public Boss(Game game, Matrix inWorldMatrix, Model inModel, Octree octree, ItemManager itemManager, ContentManager Content, List<Vector3> path) : base(game, inWorldMatrix, inModel, octree, itemManager, Content, path)
        {
            type = ObjectType.Enemy;
            animatedModel = new AnimatedModel("Models/boss");
            animatedModel.LoadContent(Content);

            walk = new AnimatedModel("Models/boss_punch");
            walk.LoadContent(Content);
            AnimationClip clip = walk.Clips[0];

            player = animatedModel.PlayClip(clip);
            player.Looping = false;

            
            boundingBox = CollisionBox.CreateBoundingBox(animatedModel.Model, position, 1);
            a = boundingBox.Max.Y - boundingBox.Min.Y;
            b = position.Y - boundingBox.Min.Y;
            //positionray = new Ray(new Vector3(position.X, boundingBox.Min.Y, position.Z), Vector3.Down);
            targetRotation = -MathHelper.PiOver2;
            speed = 55;
            maxHealth = 10000;
            currentHealth = 10000;
            worldMatrix = Matrix.CreateRotationY(targetRotation) * Matrix.CreateTranslation(position);
            boundingSphere = new BoundingSphere(position, 800);
        }

        public override bool Update(GameTime gameTime)
        {
            if (cam != null && !done)
            {
                cam.Position = new Vector3(1171.50f, 110, 1906.71f);
                cam.Rotate(-90 - cam.HeadingDegrees, 15 - cam.PitchDegrees);
                //throw new Exception(cam.Orientation.ToString());
                
                done = true;
            }

            if(done)
            { 
                Ray ray = new Ray(position, Vector3.Down);
                IntersectionRecord ir = octree.HighestIntersection(ray, DrawableObject.ObjectType.Tile);
                if (ir != null && ir.DrawableObjectObject != null)
                {
                    position.Y = ir.DrawableObjectObject.BoundingBox.Max.Y;
                    worldMatrix = Matrix.CreateRotationY(targetRotation) * Matrix.CreateTranslation(position);
                }

                //bool ret = base.Update(gameTime);
                if (dissolveAmount >= 0)
                {
                    float elapsedTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
                    spawnAge += elapsedTime;
                    dissolveAmount = MathHelper.Lerp(1, 0, spawnAge / spawnLength);
                }
                if (dupy)
                {
                    animatedModel.Update(gameTime);
                }


                if (Math.Truncate(player.Position) > counter && dupy)
                {
                    //throw new Exception(player.Duration.ToString());
                    foreach (IntersectionRecord obj in octree.AllIntersections(boundingSphere))
                    {
                        obj.DrawableObjectObject.IsStatic = false;
                        obj.DrawableObjectObject.Velocity = new Vector3(0, 70 - obj.DrawableObjectObject.Position.Y, 0);
                    }
                    counter++;
                }

                if (counter > 3)
                {
                    dupy = false;
                    foreach (IntersectionRecord obj in octree.AllIntersections(boundingSphere))
                    {
                        obj.DrawableObjectObject.IsStatic = true;
                    }
                    Game1 temp = (Game1)Game;
                    temp.ScreenManager.AddScreen(new Credits());
                }
            }

            return true;
        }

        public override void Draw(Camera camera)
        {
            cam = camera;
            /*
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (Effect effect in mesh.Effects)
                {
                    effect.CurrentTechnique = effect.Techniques["Technique1"];
                    effect.Parameters["World"].SetValue(modelBones[mesh.ParentBone.Index] * worldMatrix);
                    effect.Parameters["View"].SetValue(camera.ViewMatrix);
                    effect.Parameters["Projection"].SetValue(camera.ProjectionMatrix);
                    //effect.Parameters["FarClip"].SetValue(camera.FarZ);
                }
                mesh.Draw();
            }
            */
            Vector4 OverlayColor = Color.White.ToVector4();
            if (chilled)
                OverlayColor = Color.SlateBlue.ToVector4();
            animatedModel.Draw(GraphicsDevice, camera, worldMatrix, Content, GameplayScreen.assetContentContainer.enemyFlyTexture, OverlayColor, dissolveAmount);
            //boundingBox = CollisionBox.CreateBoundingBox(animatedModel, position, 1, Matrix.CreateFromQuaternion(Orientation)); // dostosowywany boundingbox
        }



    }
}
