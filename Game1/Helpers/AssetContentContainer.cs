using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game1.Helpers
{
    public class AssetContentContainer
    {
        public Texture2D dissolveTexture;
        public Texture2D edgeTexture;

        public Model pinetreeModel;
        public Texture2D pinetreeTexture;
        public Model tree1Model;
        public Texture2D tree1Texture;
        public Model tree2Model;
        public Texture2D tree2Texture;
        public Model treetrunkModel;
        public Texture2D treetrunkTexture;
        public Model rockModel;
        public Texture2D rockTexture;
        public Model rock1Model;
        public Model rock2Model;
        public Model rock3Model;
        public Model rock4Model;
        public Model rock5Model;
        public Model rock6Model;
        public Model rock7Model;
        public Model spawnModel;
        public Texture2D spawnTexture;

        public Texture2D pinetreeIcon;
        public Texture2D tree1Icon;
        public Texture2D tree2Icon;
        public Texture2D treetrunkIcon;
        public Texture2D rockIcon;
        public Texture2D spawnIcon;
        public Texture2D coreIcon;
        public Texture2D enemyIcon;
        public Texture2D pathIcon;
        public Texture2D turretIcon;


        public Model enemyFly;
        public Texture2D enemyFlyTexture;
        public Texture2D enemyGremlinTexture;
        public Texture2D enemyWiredTexture;
        public Texture2D enemyWalkTexture;


        public SoundEffect fireball;
        public SoundEffect icebolt;
        public SoundEffect fire2;
        public SoundEffect ice2;
        public SoundEffect teleport;
        public SoundEffect earth;
        public SoundEffect bosspunch;



        public AssetContentContainer()
        {
            SoundEffect.MasterVolume = 1.0f;
            SoundEffect.DistanceScale = 25.0f;  //10 jest głośniej do 25
        }

        public void LoadContent(ContentManager Content)
        {
            dissolveTexture = Content.Load<Texture2D>("Textures/dissolveTexture");
            edgeTexture = Content.Load<Texture2D>("Textures/edgeTexture");

            pinetreeModel = Content.Load<Model>("Models/trees/pinetree");
            pinetreeTexture = Content.Load<Texture2D>("Textures/trees/pinetree");
            tree1Model = Content.Load<Model>("Models/trees/tree1");
            tree1Texture = Content.Load<Texture2D>("Textures/trees/tree1");
            tree2Model = Content.Load<Model>("Models/trees/tree2");
            tree2Texture = Content.Load<Texture2D>("Textures/trees/tree2");
            treetrunkModel = Content.Load<Model>("Models/trees/treetrunk");
            treetrunkTexture = Content.Load<Texture2D>("Textures/trees/treetrunk");
            rockModel = Content.Load<Model>("Models/rocks/rock");
            rockTexture = Content.Load<Texture2D>("Textures/rock");
            rock1Model = Content.Load<Model>("Models/rocks/rock1");
            rock2Model = Content.Load<Model>("Models/rocks/rock2");
            rock3Model = Content.Load<Model>("Models/rocks/rock3");
            rock4Model = Content.Load<Model>("Models/rocks/rock4");
            rock5Model = Content.Load<Model>("Models/rocks/rock5");
            rock6Model = Content.Load<Model>("Models/rocks/rock6");
            rock7Model = Content.Load<Model>("Models/rocks/rock7");
            spawnModel = Content.Load<Model>("Models/core");
            spawnTexture = Content.Load<Texture2D>("Textures/core");
            enemyFly = Content.Load<Model>("Models/ship/enemy_ship");
            enemyFlyTexture = Content.Load<Texture2D>("Textures/ship/enemy_ship");

            enemyGremlinTexture = Content.Load<Texture2D>("Textures/enemies/gremlin");
            enemyWiredTexture = Content.Load<Texture2D>("Textures/enemies/wired");
            enemyWalkTexture = Content.Load<Texture2D>("Textures/enemies/walk");

            pinetreeIcon = Content.Load<Texture2D>("Interface/Map/pinetreeIcon1");
            tree1Icon = Content.Load<Texture2D>("Interface/Map/treeIcon1");
            tree2Icon = Content.Load<Texture2D>("Interface/Map/treeIcon2");
            treetrunkIcon = Content.Load<Texture2D>("Interface/Map/treetrunkIcon");
            rockIcon = Content.Load<Texture2D>("Interface/Map/rockIcon4");
            spawnIcon = Content.Load<Texture2D>("Interface/Map/spawnIcon");
            coreIcon = Content.Load<Texture2D>("Interface/Map/coreIcon");
            enemyIcon = Content.Load<Texture2D>("Interface/Map/enemyIcon");
            pathIcon = Content.Load<Texture2D>("Interface/Map/pathIcon");
            turretIcon = Content.Load<Texture2D>("Interface/Map/turretIcon");

            fireball = Content.Load<SoundEffect>("Sounds/fireball");
            icebolt = Content.Load<SoundEffect>("Sounds/icebolt");
            fire2 = Content.Load<SoundEffect>("Sounds/fire2");
            ice2 = Content.Load<SoundEffect>("Sounds/ice2");
            teleport = Content.Load<SoundEffect>("Sounds/teleport");
            earth = Content.Load<SoundEffect>("Sounds/earth");
            bosspunch = Content.Load<SoundEffect>("Sounds/bosspunch");
        }
    }
}
