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
    public class EnemyWeird : EnemyWalk
    {

        public EnemyWeird(Game game, Matrix inWorldMatrix, Model inModel, Octree octree, ItemManager itemManager, ContentManager Content, List<Vector3> path) : base(game, inWorldMatrix, inModel, octree, itemManager, Content, path)
        {
            animatedModel = new AnimatedModel("Models/weird");
            animatedModel.LoadContent(Content);

            walk = new AnimatedModel("Models/weird_walk");
            walk.LoadContent(Content);
            AnimationClip clip = walk.Clips[0];
            
            AnimationPlayer player = animatedModel.PlayClip(clip);
            player.Looping = true;

            boundingBox = CollisionBox.CreateBoundingBox(animatedModel.Model, position, 1);
            a = boundingBox.Max.Y - boundingBox.Min.Y;
            b = position.Y - boundingBox.Min.Y;
            //positionray = new Ray(new Vector3(position.X, boundingBox.Min.Y, position.Z), Vector3.Down);

            targetRotation = 0;
            speed = 45;
            maxHealth = 75;
            currentHealth = 75;
        }

    }
}
