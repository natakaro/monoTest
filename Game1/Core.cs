using Game1.Helpers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game1
{
    class Core : DrawableObject
    {
        public Core(Game game, Matrix inWorldMatrix, Model inModel, Octree octree) : base(game, inWorldMatrix, inModel, octree)
        {
            //boundingSphere = new BoundingSphere(position, Map.scale * 0.75f);

            type = ObjectType.Unit;

            boundingBox = CollisionBox.CreateBoundingBox(model, position, 1);
        }
    }
}
