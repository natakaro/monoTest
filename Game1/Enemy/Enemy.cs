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

namespace Game1
{
    class Enemy : DrawableObject
    {
        //Ray positionray;
        float gravity = 100f;
        float distance;

        float speed = 70;
        float a;
        float b;
        public float feetheight;
        ContentManager Content;
        private AnimatedModel modell = null;
        private AnimatedModel dance = null;

        private Tile targetTile;
        private int tileNumber;

        public Enemy(Game game, Matrix inWorldMatrix, Model inModel, Octree octree, ContentManager Content) : base(game, inWorldMatrix, inModel, octree)
        {
            this.Content = Content;
            m_static = true;
            //boundingSphere = new BoundingSphere(position, Map.scale * 0.75f);
           

            modell = new AnimatedModel("Models/dude");
            modell.LoadContent(Content);


            dance = new AnimatedModel("Models/dude");
            dance.LoadContent(Content);
            AnimationClip clip = dance.Clips[0];

            AnimationPlayer player = modell.PlayClip(clip);
            player.Looping = true;

            type = ObjectType.Enemy;
            boundingBox = CollisionBox.CreateBoundingBox(modell.Model, position, 1);
            a = boundingBox.Max.Y - boundingBox.Min.Y;
            b = position.Y - boundingBox.Min.Y;
            //positionray = new Ray(new Vector3(position.X, boundingBox.Min.Y, position.Z), Vector3.Down);

            tileNumber = 0;
        }

        public bool Update(GameTime gameTime, Octree octree, List <Tile> path)
        {
            Dictionary<AxialCoordinate, Tile> map = Game1.tileDictionary;
            if (path.Count != 0)
            {
                try
                {
                    targetTile = path[tileNumber];
                    Vector3 temp;
                    temp = Vector3.Normalize(targetTile.Position - position);
                    Vector3 temp2;
                    temp2 = new Vector3(temp.X, 0, temp.Z);
                    position += speed * temp2 * (float)(gameTime.ElapsedGameTime.TotalSeconds);
                    boundingBox.Max += speed * temp2 * (float)(gameTime.ElapsedGameTime.TotalSeconds);
                    boundingBox.Min += speed * temp2 * (float)(gameTime.ElapsedGameTime.TotalSeconds);
                    //IntersectionRecord ir = octree.HighestIntersection(this, ObjectType.Terrain);
                    //
                    //if (ir != null && ir.DrawableObjectObject != null)//..ujowy if ale działa
                    //{
                    //    distance = boundingBox.Min.Y - ir.DrawableObjectObject.BoundingBox.Max.Y;
                    //    boundingBox.Min.Y -= distance * gravity/10 * (float)(gameTime.ElapsedGameTime.TotalSeconds);
                    //}
                    //if (ir.DrawableObjectObject == null)
                    //    boundingBox.Min.Y -= gravity*(float)(gameTime.ElapsedGameTime.TotalSeconds);

                    Tile tile = tileFromPosition(position, map);

                    if (tile != null)
                    {
                        distance = boundingBox.Min.Y - tile.BoundingBox.Max.Y;
                        boundingBox.Min.Y -= distance * gravity / 10 * (float)(gameTime.ElapsedGameTime.TotalSeconds);
                    }
                    if (tile == null)
                        boundingBox.Min.Y -= gravity * (float)(gameTime.ElapsedGameTime.TotalSeconds);

                    boundingBox.Max.Y = boundingBox.Min.Y + a;
                    position.Y = boundingBox.Min.Y + b;

                    float targetrotation = (float)Math.Atan2((double)(targetTile.Position.Y - position.Y), (double)(targetTile.Position.X - position.X));
                    worldMatrix = Matrix.CreateRotationY(targetrotation) * Matrix.CreateTranslation(position);
                    modell.Update(gameTime);

                    if (Math.Abs(position.X - targetTile.Position.X) < 5 && Math.Abs(position.Z - targetTile.Position.Z) < 5 && tileNumber < path.Count)
                    {
                        tileNumber++;
                    }
                }
                catch
                {
                    alive = false;
                }

            }
                

            return true;

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
            modell.Draw(GraphicsDevice, camera, worldMatrix, Content);
        }
    }
}
