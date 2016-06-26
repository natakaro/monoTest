using Game1.Lights;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game1.Items
{
    public class ItemManager
    {
        private Game game;
        private ContentManager content;
        private ObjectManager objectManager;
        private Octree octree;
        private LightManager lightManager;
        private Stats stats;
         
        private Model essenceModel;
        private Texture2D essenceTexture;

        private Model heartModel;
        private Texture2D heartTexture;

        private List<Item> list;
        private List<Item> toAdd;
        private List<Item> toRemove;

        public List<Item> List
        {
            get { return list; }
        }

        public ItemManager(Game game, ContentManager content, Octree octree, ObjectManager objectManager, LightManager lightManager, Stats stats)
        {
            this.game = game;
            this.content = content;
            this.octree = octree;
            this.objectManager = objectManager;
            this.lightManager = lightManager;
            this.stats = stats;

            list = new List<Item>();
            toAdd = new List<Item>();
            toRemove = new List<Item>();

            essenceModel = content.Load<Model>("Models/crystal");
            essenceTexture = content.Load<Texture2D>("Textures/crystal");

            heartModel = content.Load<Model>("Models/crystal");
            heartTexture = content.Load<Texture2D>("Textures/heart");
        }

        public void Update(GameTime gameTime, Vector3 cameraPosition)
        {
            foreach (Item item in toRemove)
            {
                list.Remove(item);
            }
            list.AddRange(toAdd);

            toRemove.Clear();
            toAdd.Clear();

            foreach (Item item in list)
            {
                item.Update(gameTime);
                float distance = Vector3.Distance(cameraPosition, item.Position);
                if (distance < 50)
                    item.PickUp();
            }
        }

        public void Draw(Camera camera)
        {
            foreach (Item item in list)
            {
                item.Draw(camera);
            }
        }

        public void Add(Item item)
        {
            toAdd.Add(item);
        }

        public void Remove(Item item)
        {
            toRemove.Add(item);
        }


        public void SpawnEssence(Vector3 position)
        {
            float randomAngle = Game1.random.Next(0, 359);
            Matrix worldMatrix = Matrix.CreateRotationY(MathHelper.ToRadians(randomAngle)) * Matrix.CreateTranslation(position);

            Essence item = new Essence(game, worldMatrix, essenceModel, octree, this, essenceTexture, lightManager, stats);
            item.Position = position;
            Add(item);
        }

        public void SpawnHeart(Vector3 position)
        {
            float randomAngle = Game1.random.Next(0, 359);
            Matrix worldMatrix = Matrix.CreateRotationY(MathHelper.ToRadians(randomAngle)) * Matrix.CreateTranslation(position);

            Heart item = new Heart(game, worldMatrix, heartModel, octree, this, heartTexture, lightManager, stats);
            item.Position = position;
            Add(item);
        }
    }
}
