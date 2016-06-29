using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Game1.Lights;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Game1.Items
{
    class Heart : Item
    {
        public Heart(Game game, Matrix inWorldMatrix, Model inModel, Octree octree, ItemManager itemManager, Texture2D inTexture, LightManager lightManager, Stats stats) : base(game, inWorldMatrix, inModel, octree, itemManager, inTexture, lightManager, stats)
        {
            pointLight = new PointLight(position + new Vector3(0, 3, 0), Color.Red, 5, 5);
            lightManager.AddLight(pointLight);
        }

        public override void PickUp()
        {
            if (pickedUp == false)
            {
                if (stats.currentHealth < stats.maxHealth)
                {
                    stats.currentHealth = Math.Min(stats.currentHealth + 10, stats.maxHealth);
                    pickedUp = true;
                }
            }
        }

        public override void Destroy()
        {
            lightManager.RemoveLight(pointLight);
            base.Destroy();
        }
    }
}
