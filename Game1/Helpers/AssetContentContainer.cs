﻿using Microsoft.Xna.Framework.Content;
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

        public AssetContentContainer()
        {

        }

        public void LoadContent(ContentManager Content)
        {
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
        }
    }
}
