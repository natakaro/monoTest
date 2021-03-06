﻿using System;
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
    public class EnemyWalk : Enemy
    {

        protected AnimatedModel animatedModel = null;
        protected AnimatedModel walk = null;
        protected Texture2D texture = null;
        protected Sound sound;

        public EnemyWalk(Game game, Matrix inWorldMatrix, Model inModel, Octree octree, ItemManager itemManager, ContentManager Content, List<Vector3> path) : base(game, inWorldMatrix, inModel, octree, itemManager, Content, path)
        {
            type = ObjectType.Enemy;
            animatedModel = new AnimatedModel("Models/h");
            animatedModel.LoadContent(Content);

            walk = new AnimatedModel("Models/h_walk");
            walk.LoadContent(Content);
            AnimationClip clip = walk.Clips[0];
            
            AnimationPlayer player = animatedModel.PlayClip(clip);
            player.Looping = true;

            texture = GameplayScreen.assetContentContainer.enemyWalkTexture;

            boundingBox = CollisionBox.CreateBoundingBox(animatedModel.Model, position, 1);
            a = boundingBox.Max.Y - boundingBox.Min.Y;
            b = position.Y - boundingBox.Min.Y;
            //positionray = new Ray(new Vector3(position.X, boundingBox.Min.Y, position.Z), Vector3.Down);

            targetRotation = 0;
            speed = 65;
            maxHealth = 100;
            currentHealth = 100;
            chilledLengthMax = 8;

            sound = new Sound(GameplayScreen.assetContentContainer.teleport);
            sound.soundInstance.Volume = 0.1f;
            sound.Play(position);
        }

        public override bool Update(GameTime gameTime)
        {
            bool ret = base.Update(gameTime);

            if (CurrentHealth <= 0)
            {
                Die(gameTime);
            }
            else
            {
                float elapsedTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

                Dictionary<AxialCoordinate, Tile> map = GameplayScreen.map;

                if (path.Count - tileNumber > 0)
                {
                    Vector3 targetPosition = path[tileNumber];
                    Vector3 direction = Vector3.Normalize(targetPosition - position);
                    Vector3 distance = new Vector3(targetPosition.X, 0, targetPosition.Z) - new Vector3(position.X, 0, position.Z);
                    Vector3 directionXZ = Vector3.Normalize(distance);
                    velocity = speed * direction;

                    if (chilled)
                    {
                        velocity /= 2;
                        chilledAge += elapsedTime;

                        if (chilledAge > chilledLength)
                        {
                            chilled = false;
                            chilledAge = 0;
                        }
                    }

                    //Vector3 dist2 = path[tileNumber - 1] - position;
                    //position.Y += 5 * dist2.Y * (float)(gameTime.ElapsedGameTime.TotalSeconds);

                    //Tile tile = tileFromPosition(position, map);

                    //if (tile != null)
                    //{
                    //    distance = boundingBox.Min.Y - tile.BoundingBox.Max.Y;
                    //    boundingBox.Min.Y -= distance * gravity / 10 * (float)(gameTime.ElapsedGameTime.TotalSeconds);
                    //}
                    //if (tile == null)
                    //    boundingBox.Min.Y -= gravity * (float)(gameTime.ElapsedGameTime.TotalSeconds);

                    //boundingBox.Max.Y = boundingBox.Min.Y + a;
                    //position.Y = boundingBox.Min.Y + b;

                    targetRotation = (float)Math.Atan2((double)(targetPosition.X - position.X), (double)(targetPosition.Z - position.Z));
                    worldMatrix = Matrix.CreateRotationY(targetRotation) * Matrix.CreateTranslation(position);
                    Orientation = worldMatrix.Rotation;
                    animatedModel.Update(gameTime);

                    /*
                    ////wyciagamy punkty 
                    Vector3[] obb = new Vector3[8];
                    obb = boundingBox.GetCorners();

                    ////transformacja o worldmatrix
                    Matrix rot = Matrix.CreateRotationY(targetRotation);
                    Vector3.Transform(obb, ref rot, obb);
                    ////tworzymy boxa na podstawie punktow 
                    boundingBox = BoundingBox.CreateFromPoints(obb);
                    */

                    //while (Vector3.Distance(position, targetPosition) < 25 && tileNumber < path.Count - 1)
                    //{
                    //tileNumber++;
                    //targetPosition = path[tileNumber];
                    //}

                    if (Vector3.Distance(position, targetPosition) < 30 && tileNumber < path.Count)
                    {
                        tileNumber++;
                    }
                }
                else
                    EnterCore(gameTime);
            }

            sound.Update(position);
            return ret;
        }

        public override void Draw(Camera camera)
        {
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
            animatedModel.Draw(GraphicsDevice, camera, worldMatrix, Content, texture, OverlayColor, dissolveAmount);
            //boundingBox = CollisionBox.CreateBoundingBox(animatedModel, position, 1, Matrix.CreateFromQuaternion(Orientation)); // dostosowywany boundingbox
        }

        public override void Die(GameTime gameTime)
        {
            //animatedModel.Update(gameTime);
            velocity = Vector3.Zero;
            worldMatrix = Matrix.CreateRotationY(targetRotation) * Matrix.CreateTranslation(position);
            Orientation = worldMatrix.Rotation;
            base.Die(gameTime);
        }

        public override void EnterCore(GameTime gameTime)
        {
            //animatedModel.Update(gameTime);
            velocity = Vector3.Zero;
            worldMatrix = Matrix.CreateRotationY(targetRotation) * Matrix.CreateTranslation(position);
            Orientation = worldMatrix.Rotation;
            base.EnterCore(gameTime);
        }

        public AnimatedModel AnimatedModel
        {
            get { return animatedModel; }
        }

    }
}
