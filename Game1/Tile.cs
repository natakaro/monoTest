﻿using Game1.Helpers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game1
{
    class Tile : DrawableObject
    {
        public Tile(Game game, Matrix inWorldMatrix, Model inModel) : base(game, inWorldMatrix, inModel)
        {
            m_instanced = true;
            //boundingSphere = new BoundingSphere(position, Map.scale * 0.75f);
            
            type = ObjectType.Terrain;

            boundingBox = CollisionBox.CreateBoundingBox(model, position, 1);
        }
    }
}
