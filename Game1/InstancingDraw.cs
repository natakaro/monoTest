using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;

namespace Game1
{

    public class InstancingDraw: GameComponent
    {


        Effect effect;

        //List<IntersectionRecord> instances;
        //Matrix[] instanceTransforms;
        Model model;
        Matrix[] modelBones;
        DynamicVertexBuffer instanceVertexBuffer;
        GraphicsDevice graphicsDevice;
        Camera camera;
        Texture2D tex;
        Matrix[] instances;


        // To store instance transform matrices in a vertex buffer, we use this custom
        // vertex type which encodes 4x4 matrices as a set of four Vector4 values.
        static VertexDeclaration instanceVertexDeclaration = new VertexDeclaration
        (
            new VertexElement(0, VertexElementFormat.Vector4, VertexElementUsage.BlendWeight, 0),
            new VertexElement(16, VertexElementFormat.Vector4, VertexElementUsage.BlendWeight, 1),
            new VertexElement(32, VertexElementFormat.Vector4, VertexElementUsage.BlendWeight, 2),
            new VertexElement(48, VertexElementFormat.Vector4, VertexElementUsage.BlendWeight, 3)
        );



        public InstancingDraw(Game1 game, Camera camera, ContentManager contentManager) : base(game)
        {
            // Initialize the list of instances.
            graphicsDevice = game.GraphicsDevice;
            model = contentManager.Load<Model>("1");
            //effect = contentManager.Load<Effect>("Effects/RenderGBuffer");
            tex = contentManager.Load<Texture2D>("gradient");
            this.camera = camera;
            //instances = insta;
            modelBones = new Matrix[model.Bones.Count];
            model.CopyAbsoluteBoneTransformsTo(modelBones);
            //Count();
        }


        /*
        protected void Count(List<IntersectionRecord> instances)
        {
            Matrix temp = Matrix.CreateScale(Map.scale); //tylko do Tile
            // Gather instance transform matrices into a single array.
            Array.Resize(ref instanceTransforms, instances.Count);

            for (int i = 0; i < instances.Count; i++)
            {
                instanceTransforms[i] = temp * Matrix.CreateTranslation(instances[i].DrawableObjectObject.Position);
            }

        }
        */

        /// <summary>
        /// Efficiently draws several copies of a piece of geometry using hardware instancing.
        /// </summary>
        ///
        
        public void Draw()
        {
            //DrawModelHardwareInstancing(instancedModel, instancedModelBones, instanceTransforms);
        }

        public void DrawModelHardwareInstancing(List<IntersectionRecord> insta)
        {
            
            // Gather instance transform matrices into a single array.
            Array.Resize(ref instances, insta.Count);

            for (int i = 0; i < insta.Count; i++)
            {
                instances[i] = Matrix.CreateTranslation(insta[i].DrawableObjectObject.Position);
            }

            if (instances.Length == 0)
                return;

            // If we have more instances than room in our vertex buffer, grow it to the neccessary size.
            if ((instanceVertexBuffer == null) ||
                (instances.Length > instanceVertexBuffer.VertexCount))
            {
                if (instanceVertexBuffer != null)
                    instanceVertexBuffer.Dispose();

                instanceVertexBuffer = new DynamicVertexBuffer(graphicsDevice, instanceVertexDeclaration,
                                                               instances.Length, BufferUsage.WriteOnly);
            }

            // Transfer the latest instance transform matrices into the instanceVertexBuffer.
            instanceVertexBuffer.SetData(instances, 0, instances.Length, SetDataOptions.Discard);

            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (Effect effect in mesh.Effects)
                {
                    foreach (ModelMeshPart meshPart in mesh.MeshParts)
                    {
                        // Tell the GPU to read from both the model vertex buffer plus our instanceVertexBuffer.
                        graphicsDevice.SetVertexBuffers(
                            new VertexBufferBinding(meshPart.VertexBuffer, meshPart.VertexOffset, 0),
                            new VertexBufferBinding(instanceVertexBuffer, 0, 1)
                        );

                        graphicsDevice.Indices = meshPart.IndexBuffer;

                        // Set up the instance rendering effect.
                        //meshPart.Effect = effect;

                        effect.CurrentTechnique = effect.Techniques["InstancingColor"];

                        effect.Parameters["World"].SetValue(modelBones[mesh.ParentBone.Index]);
                        effect.Parameters["View"].SetValue(camera.ViewMatrix);
                        effect.Parameters["Projection"].SetValue(camera.ProjectionMatrix);
                        effect.Parameters["Texture"].SetValue(tex);

                        // Draw all the instance copies in a single call.
                        foreach (EffectPass pass in effect.CurrentTechnique.Passes)
                        {
                            pass.Apply();

                            graphicsDevice.DrawInstancedPrimitives(PrimitiveType.TriangleList, 0, 0,
                                                                   meshPart.NumVertices, meshPart.StartIndex,
                                                                   meshPart.PrimitiveCount, instances.Length);
                        }
                    }
                }
            }
        }

    }
}
