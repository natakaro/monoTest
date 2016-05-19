using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using AnimationAux;
using System.ComponentModel;
using System.IO;


namespace AnimationPipeline
{
    /// <summary>
    /// This class extends the standard ModelProcessor to include code
    /// that extracts a skeleton, pulls any animations, and does any
    /// necessary prep work to support animation and skinning.
    /// </summary>
    [ContentProcessor(DisplayName = "Animation Processor")]
    public class AnimationProcessor : ModelProcessor
    {
        /// <summary>
        /// The model we are reading
        /// </summary>
        private ModelContent model;

        /// <summary>
        /// Extra content to associated with the model. This is where we put the stuff that is 
        /// unique to this project.
        /// </summary>
        private ModelExtra modelExtra = new ModelExtra();

        /// <summary>
        /// A lookup dictionary that remembers when we changes a material to 
        /// skinned material.
        /// </summary>
        private Dictionary<MaterialContent, SkinnedMaterialContent> toSkinnedMaterial = new Dictionary<MaterialContent, SkinnedMaterialContent>();

        /// <summary>
        /// The function to process a model from original content into model content for export
        /// </summary>
        /// <param name="input"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public override ModelContent Process(NodeContent input, ContentProcessorContext context)
        {
            
            if (input == null)
            {
                throw new ArgumentNullException("input");
            }
            directory = Path.GetDirectoryName(input.Identity.SourceFilename);
            LookUpTextures(input);
            //return base.Process(input, context);
            // Process the skeleton for skinned character animation
            BoneContent skeleton = ProcessSkeleton(input);

            //SwapSkinnedMaterial(input);
            
            model = base.Process(input, context);
            ProcessAnimations(model, input, context);

            /*
            if (input == null)
            {
                throw new ArgumentNullException("input");
            }
            directory = Path.GetDirectoryName(input.Identity.SourceFilename);
            LookUpTextures(input);*/

            // Add the extra content to the model 
            model.Tag = modelExtra;

            return model;
        }

        #region Skeleton Support

        /// <summary>
        /// Process the skeleton in support of skeletal animation...
        /// </summary>
        /// <param name="input"></param>
        private BoneContent ProcessSkeleton(NodeContent input)
        {
            // Find the skeleton.
            BoneContent skeleton = MeshHelper.FindSkeleton(input);

            if (skeleton == null)
                return null;

            // We don't want to have to worry about different parts of the model being
            // in different local coordinate systems, so let's just bake everything.
            FlattenTransforms(input, skeleton);

            //
            // 3D Studio Max includes helper bones that end with "Nub"
            // These are not part of the skinning system and can be 
            // discarded.  TrimSkeleton removes them from the geometry.
            //

            TrimSkeleton(skeleton);

            // Convert the heirarchy of nodes and bones into a list
            List<NodeContent> nodes = FlattenHeirarchy(input);
            IList<BoneContent> bones = MeshHelper.FlattenSkeleton(skeleton);

            // Create a dictionary to convert a node to an index into the array of nodes
            Dictionary<NodeContent, int> nodeToIndex = new Dictionary<NodeContent, int>();
            for (int i = 0; i < nodes.Count; i++)
            {
                nodeToIndex[nodes[i]] = i;
            }

            // Now create the array that maps the bones to the nodes
            foreach (BoneContent bone in bones)
            {
                modelExtra.Skeleton.Add(nodeToIndex[bone]);
            }

            return skeleton;
        }

        /// <summary>
        /// Convert a tree of nodes into a list of nodes in topological order.
        /// </summary>
        /// <param name="item">The root of the heirarchy</param>
        /// <returns></returns>
        private List<NodeContent> FlattenHeirarchy(NodeContent item)
        {
            List<NodeContent> nodes = new List<NodeContent>();
            nodes.Add(item);
            foreach (NodeContent child in item.Children)
            {
                FlattenHeirarchy(nodes, child);
            }

            return nodes;
        }


        private void FlattenHeirarchy(List<NodeContent> nodes, NodeContent item)
        {
            nodes.Add(item);
            foreach (NodeContent child in item.Children)
            {
                FlattenHeirarchy(nodes, child);
            }
        }

        /// <summary>
        /// Bakes unwanted transforms into the model geometry,
        /// so everything ends up in the same coordinate system.
        /// </summary>
        void FlattenTransforms(NodeContent node, BoneContent skeleton)
        {
            foreach (NodeContent child in node.Children)
            {
                // Don't process the skeleton, because that is special.
                if (child == skeleton)
                    continue;

                // This is important: Don't bake in the transforms except
                // for geometry that is part of a skinned mesh
                if(IsSkinned(child))
                {
                    FlattenAllTransforms(child);
                }
            }
        }

        /// <summary>
        /// Recursively flatten all transforms from this node down
        /// </summary>
        /// <param name="node"></param>
        void FlattenAllTransforms(NodeContent node)
        {
            // Bake the local transform into the actual geometry.
            MeshHelper.TransformScene(node, node.Transform);

            // Having baked it, we can now set the local
            // coordinate system back to identity.
            node.Transform = Matrix.Identity;

            foreach (NodeContent child in node.Children)
            {
                FlattenAllTransforms(child);
            }
        }

        /// <summary>
        /// 3D Studio Max includes an extra help bone at the end of each
        /// IK chain that doesn't effect the skinning system and is 
        /// redundant as far as any game is concerned.  This function
        /// looks for children who's name ends with "Nub" and removes
        /// them from the heirarchy.
        /// </summary>
        /// <param name="skeleton">Root of the skeleton tree</param>
        void TrimSkeleton(NodeContent skeleton)
        {
            List<NodeContent> todelete = new List<NodeContent>();

            foreach (NodeContent child in skeleton.Children)
            {
                if (child.Name.EndsWith("Nub") || child.Name.EndsWith("Footsteps"))
                    todelete.Add(child);
                else
                    TrimSkeleton(child);
            }

            foreach (NodeContent child in todelete)
            {
                skeleton.Children.Remove(child);
            }
        }


        #endregion

        #region Skinned Support

        /// <summary>
        /// Determine if a node is a skinned node, meaning it has bone weights
        /// associated with it.
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        bool IsSkinned(NodeContent node)
        {
            // It has to be a MeshContent node
            MeshContent mesh = node as MeshContent;
            if (mesh != null)
            {
                // In the geometry we have to find a vertex channel that
                // has a bone weight collection
                foreach (GeometryContent geometry in mesh.Geometry)
                {
                    foreach (VertexChannel vchannel in geometry.Vertices.Channels)
                    {
                        if (vchannel is VertexChannel<BoneWeightCollection>)
                            return true;
                    }
                }
            }

            return false;
        }


        /// <summary>
        /// If a node is skinned, we need to use the skinned model 
        /// effect rather than basic effect. This function runs through the 
        /// geometry and finds the meshes that have bone weights associated 
        /// and swaps in the skinned effect. 
        /// </summary>
        /// <param name="node"></param>
        void SwapSkinnedMaterial(NodeContent node)
        {
            // It has to be a MeshContent node
            MeshContent mesh = node as MeshContent;
            if (mesh != null)
            {
                // In the geometry we have to find a vertex channel that
                // has a bone weight collection
                foreach (GeometryContent geometry in mesh.Geometry)
                {
                    bool swap = false;
                    foreach (VertexChannel vchannel in geometry.Vertices.Channels)
                    {
                        if (vchannel is VertexChannel<BoneWeightCollection>)
                        {
                            swap = true;
                            break;
                        }
                    }

                    if (swap)
                    {
                        if (toSkinnedMaterial.ContainsKey(geometry.Material))
                        {
                            // We have already swapped it
                            geometry.Material = toSkinnedMaterial[geometry.Material];
                        }
                        else
                        {
                            SkinnedMaterialContent smaterial = new SkinnedMaterialContent();
                            BasicMaterialContent bmaterial = geometry.Material as BasicMaterialContent;

                            // Copy over the data
                            smaterial.Alpha = bmaterial.Alpha;
                            smaterial.DiffuseColor = bmaterial.DiffuseColor;
                            smaterial.EmissiveColor = bmaterial.EmissiveColor;
                            smaterial.SpecularColor = bmaterial.SpecularColor;
                            smaterial.SpecularPower = bmaterial.SpecularPower;
                            smaterial.Texture = bmaterial.Texture;
                            smaterial.WeightsPerVertex = 4;

                            toSkinnedMaterial[geometry.Material] = smaterial;
                            geometry.Material = smaterial;
                        }
                    }
                }
            }

            foreach (NodeContent child in node.Children)
            {
                SwapSkinnedMaterial(child);
            }
        }

        #endregion

        #region Animation Support

        /// <summary>
        /// Bones lookup table, converts bone names to indices.
        /// </summary>
        private Dictionary<string, int> bones = new Dictionary<string, int>();

        /// <summary>
        /// This will keep track of all of the bone transforms for a base pose
        /// </summary>
        private Matrix[] boneTransforms;

        /// <summary>
        /// A dictionary so we can keep track of the clips by name
        /// </summary>
        private Dictionary<string, AnimationClip> clips = new Dictionary<string, AnimationClip>();

        /// <summary>
        /// Entry point for animation processing. 
        /// </summary>
        /// <param name="model"></param>
        /// <param name="input"></param>
        /// <param name="context"></param>
        private void ProcessAnimations(ModelContent model, NodeContent input, ContentProcessorContext context)
        {
            // First build a lookup table so we can determine the 
            // index into the list of bones from a bone name.
            for (int i = 0; i < model.Bones.Count; i++)
            {
                bones[model.Bones[i].Name] = i;
            }

            // For saving the bone transforms
            boneTransforms = new Matrix[model.Bones.Count];

            //
            // Collect up all of the animation data
            //

            ProcessAnimationsRecursive(input);

            // Ensure there is always a clip, even if none is included in the FBX
            // That way we can create poses using FBX files as one-frame 
            // animation clips
            if (modelExtra.Clips.Count == 0)
            {
                AnimationClip clip = new AnimationClip();
                modelExtra.Clips.Add(clip);

                string clipName = "Take 001";

                // Retain by name
                clips[clipName] = clip;

                clip.Name = clipName;
                foreach (ModelBoneContent bone in model.Bones)
                {
                    AnimationClip.Bone clipBone = new AnimationClip.Bone();
                    clipBone.Name = bone.Name;

                    clip.Bones.Add(clipBone);
                }
            }

            // Ensure all animations have a first key frame for every bone
            foreach (AnimationClip clip in modelExtra.Clips)
            {
                for (int b = 0; b < bones.Count; b++)
                {
                    List<AnimationClip.Keyframe> keyframes = clip.Bones[b].Keyframes;
                    if (keyframes.Count == 0 || keyframes[0].Time > 0)
                    {
                        AnimationClip.Keyframe keyframe = new AnimationClip.Keyframe();
                        keyframe.Time = 0;
                        keyframe.Transform = boneTransforms[b];
                        keyframes.Insert(0, keyframe);
                    }
                }
            }
        }

        /// <summary>
        /// Recursive function that processes the entire scene graph, collecting up
        /// all of the animation data.
        /// </summary>
        private void ProcessAnimationsRecursive(NodeContent input)
        {
            // Look up the bone for this input channel
            int inputBoneIndex;
            if (bones.TryGetValue(input.Name, out inputBoneIndex))
            {
                // Save the transform
                boneTransforms[inputBoneIndex] = input.Transform;
            }


            foreach (KeyValuePair<string, AnimationContent> animation in input.Animations)
            {
                // Do we have this animation before?
                AnimationClip clip;
                string clipName = animation.Key;

                if (!clips.TryGetValue(clipName, out clip))
                {
                    // Never seen before clip
                    clip = new AnimationClip();
                    modelExtra.Clips.Add(clip);

                    // Retain by name
                    clips[clipName] = clip;

                    clip.Name = clipName;
                    foreach (ModelBoneContent bone in model.Bones)
                    {
                        AnimationClip.Bone clipBone = new AnimationClip.Bone();
                        clipBone.Name = bone.Name;

                        clip.Bones.Add(clipBone);
                    }
                }

                // Ensure the duration is always set
                if (animation.Value.Duration.TotalSeconds > clip.Duration)
                    clip.Duration = animation.Value.Duration.TotalSeconds;

                //
                // For each channel, determine the bone and then process all of the 
                // keyframes for that bone.
                //

                foreach (KeyValuePair<string, AnimationChannel> channel in animation.Value.Channels)
                {
                    // What is the bone index?
                    int boneIndex;
                    if (!bones.TryGetValue(channel.Key, out boneIndex))
                        continue;           // Ignore if not a named bone

                    // An animation is useless if it is for a bone not assigned to any meshes at all
                    if (UselessAnimationTest(boneIndex))
                        continue;

                    // I'm collecting up in a linked list so we can process the data
                    // and remove redundant keyframes
                    LinkedList<AnimationClip.Keyframe> keyframes = new LinkedList<AnimationClip.Keyframe>();
                    foreach (AnimationKeyframe keyframe in channel.Value)
                    {
                        Matrix transform = keyframe.Transform;      // Keyframe transformation

                        AnimationClip.Keyframe newKeyframe = new AnimationClip.Keyframe();
                        newKeyframe.Time = keyframe.Time.TotalSeconds;
                        newKeyframe.Transform = transform;

                        keyframes.AddLast(newKeyframe);
                    }

                    LinearKeyframeReduction(keyframes);
                    foreach (AnimationClip.Keyframe keyframe in keyframes)
                    {
                        clip.Bones[boneIndex].Keyframes.Add(keyframe);
                    }

                }


            }

            foreach (NodeContent child in input.Children)
            {
                ProcessAnimationsRecursive(child);
            }
        }

        private const float TinyLength = 1e-8f;
        private const float TinyCosAngle = 0.9999999f;

        /// <summary>
        /// This function filters out keyframes that can be approximated well with 
        /// linear interpolation.
        /// </summary>
        /// <param name="keyframes"></param>
        private void LinearKeyframeReduction(LinkedList<AnimationClip.Keyframe> keyframes)
        {
            if (keyframes.Count < 3)
                return;

            for (LinkedListNode<AnimationClip.Keyframe> node = keyframes.First.Next; ; )
            {
                LinkedListNode<AnimationClip.Keyframe> next = node.Next;
                if (next == null)
                    break;

                // Determine nodes before and after the current node.
                AnimationClip.Keyframe a = node.Previous.Value;
                AnimationClip.Keyframe b = node.Value;
                AnimationClip.Keyframe c = next.Value;

                float t = (float)((node.Value.Time - node.Previous.Value.Time) /
                                   (next.Value.Time - node.Previous.Value.Time));

                Vector3 translation = Vector3.Lerp(a.Translation, c.Translation, t);
                Quaternion rotation = Quaternion.Slerp(a.Rotation, c.Rotation, t);

                if ((translation - b.Translation).LengthSquared() < TinyLength &&
                   Quaternion.Dot(rotation, b.Rotation) > TinyCosAngle)
                {
                    keyframes.Remove(node);
                }

                node = next;
            }
        }

        /// <summary>
        /// Discard any animation not assigned to a mesh or the skeleton
        /// </summary>
        /// <param name="boneId"></param>
        /// <returns></returns>
        bool UselessAnimationTest(int boneId)
        {
            // If any mesh is assigned to this bone, it is not useless
            foreach (ModelMeshContent mesh in model.Meshes)
            {
                if (mesh.ParentBone.Index == boneId)
                    return false;
            }

            // If this bone is in the skeleton, it is not useless
            foreach (int b in modelExtra.Skeleton)
            {
                if (boneId == b)
                    return false;
            }

            // Otherwise, it is useless
            return true;
        }

        #endregion


            String directory;
            [DisplayName("Normal Map Texture")]
            [Description("If set, this file will be used as the normal map on the model, " +
            "overriding anything found in the opaque data.")]
            [DefaultValue("")]
            public string NormalMapTexture
            {
                get { return normalMapTexture; }
                set { normalMapTexture = value; }
            }
            private string normalMapTexture;

            [DisplayName("Normal Map Key")]
            [Description("This will be the key that will be used to search the normal map in the opaque data of the model")]
            [DefaultValue("NormalMap")]
            public string NormalMapKey
            {
                get { return normalMapKey; }
                set { normalMapKey = value; }
            }
            private string normalMapKey = "NormalMap";

        /*
            public override ModelContent Process(NodeContent input, ContentProcessorContext context)
            {
                if (input == null)
                {
                    throw new ArgumentNullException("input");
                }
                directory = Path.GetDirectoryName(input.Identity.SourceFilename);
                LookUpTextures(input);
                return base.Process(input, context);
            }*/

            [Browsable(false)]
            public override bool GenerateTangentFrames
            {
                get { return true; }
                set { }
            }

            static IList<string> acceptableVertexChannelNames =
            new string[]
            {
            VertexChannelNames.TextureCoordinate(0),
            VertexChannelNames.Normal(0),
            VertexChannelNames.Binormal(0),
            VertexChannelNames.Tangent(0),
            };

            protected void ProcessVertexChannell(GeometryContent geometry, int vertexChannelIndex, ContentProcessorContext context)
            {
                String vertexChannelName =
                    geometry.Vertices.Channels[vertexChannelIndex].Name;

                // if this vertex channel has an acceptable names, process it as normal.
                if (acceptableVertexChannelNames.Contains(vertexChannelName))
                {
                    base.ProcessVertexChannel(geometry, vertexChannelIndex, context);
                }
                // otherwise, remove it from the vertex channels; it's just extra data
                // we don't need.
                else
                {
                    geometry.Vertices.Channels.Remove(vertexChannelName);
                }
            }

            private void LookUpTextures(NodeContent node)
            {
                MeshContent mesh = node as MeshContent;
                if (mesh != null)
                {
                    //this will contatin the path to the normal map texture
                    string normalMapPath = null;
                    //If the NormalMapTexture property is set, we use that normal map for all meshes in the model.
                    //This overrides anything else
                    if (!String.IsNullOrEmpty(NormalMapTexture))
                    {
                        normalMapPath = NormalMapTexture;
                    }
                    else
                    {
                        object value;
                        //If NormalMapTexture is not set, we look into the opaque data of the model, 
                        //and search for a texture with the key equal to NormalMapKey
                        if (mesh.OpaqueData.TryGetValue(NormalMapKey, out value))
                            normalMapPath = value.ToString();
                    }
                    //if the NormalMapTexture Property was not used, and the key was not found in the model, than normalMapPath would have the value null.
                    if (normalMapPath == null)
                    {
                        //If a key with the required name is not found, we make a final attempt, 
                        //and search, in the same directory as the model, for a texture named 
                        //meshname_n.tga, where meshname is the name of a mesh inside the model.
                        normalMapPath = Path.Combine(directory, mesh.Name + "Textures/_n.tga");
                        if (!File.Exists(normalMapPath))
                        {
                            //if this fails also (that texture does not exist), 
                            //then we use a default texture, named null_normal.tga
                            normalMapPath = "Textures/null_normal.tga";
                        }
                    }
                    else
                    {
                        normalMapPath = Path.Combine(directory, normalMapPath);
                    }

                    string specularMapPath = null;

                    //If the SpecularMapTexture property is set, we use it
                    if (!String.IsNullOrEmpty(SpecularMapTexture))
                    {
                        specularMapPath = SpecularMapTexture;
                    }
                    else
                    {
                        object value;
                        //If SpecularMapTexture is not set, we look into the opaque data of the model, 
                        //and search for a texture with the key equal to specularMapKey
                        if (mesh.OpaqueData.TryGetValue(specularMapKey, out value))
                            specularMapPath = value.ToString();
                    }

                    if (specularMapPath == null)
                    {
                        //we search, in the same directory as the model, for a texture named 
                        //meshname_s.tga
                        specularMapPath = Path.Combine(directory, mesh.Name + "Textures/_s.tga");
                        if (!File.Exists(specularMapPath))
                        {
                            //if this fails also (that texture does not exist), 
                            //then we use a default texture, named null_specular.tga
                            specularMapPath = "Textures/null_specular.tga";
                        }
                    }
                    else
                    {
                        specularMapPath = Path.Combine(directory, specularMapPath);
                    }
                    //add the keys to the material, so they can be used by the shader
                    foreach (GeometryContent geometry in mesh.Geometry)
                    {
                        //in some .fbx files, the key might be found in the textures collection, but not
                        //in the mesh, as we checked above. If this is the case, we need to get it out, and
                        //add it with the "NormalMap" key
                        if (geometry.Material.Textures.ContainsKey(normalMapKey))
                        {
                            ExternalReference<TextureContent> texRef = geometry.Material.Textures[normalMapKey];
                            geometry.Material.Textures.Remove(normalMapKey);
                            geometry.Material.Textures.Add("NormalMap", texRef);
                        }
                        else
                            geometry.Material.Textures.Add("NormalMap",
                                            new ExternalReference<TextureContent>(normalMapPath));

                        if (geometry.Material.Textures.ContainsKey(specularMapKey))
                        {
                            ExternalReference<TextureContent> texRef = geometry.Material.Textures[specularMapKey];
                            geometry.Material.Textures.Remove(specularMapKey);
                            geometry.Material.Textures.Add("SpecularMap", texRef);
                        }
                        else
                            geometry.Material.Textures.Add("SpecularMap",
                                        new ExternalReference<TextureContent>(specularMapPath));
                    }
                }

                // go through all children and apply LookUpTextures recursively
                foreach (NodeContent child in node.Children)
                {
                    LookUpTextures(child);
                }
            }

            [DisplayName("Specular Map Texture")]
            [Description("If set, this file will be used as the specular map on the model, " +
            "overriding anything found in the opaque data.")]
            [DefaultValue("")]
            public string SpecularMapTexture
            {
                get { return specularMapTexture; }
                set { specularMapTexture = value; }
            }
            private string specularMapTexture;

            [DisplayName("Specular Map Key")]
            [Description("This will be the key that will be used to search the specular map in the opaque data of the model")]
            [DefaultValue("SpecularMap")]
            public string SpecularMapKey
            {
                get { return specularMapKey; }
                set { specularMapKey = value; }
            }
            private string specularMapKey = "SpecularMap";

        protected override MaterialContent ConvertMaterial(MaterialContent material,
                                                                               ContentProcessorContext context)
        {

            EffectMaterialContent deferredShadingMaterial = new EffectMaterialContent();

            if (DefaultEffect.ToString() == "SkinnedEffect")
            {

                deferredShadingMaterial.Effect = new ExternalReference<EffectContent>("Effects/RenderGAnimation.fx");
            }
            else
            {
                deferredShadingMaterial.Effect = new ExternalReference<EffectContent>("Effects/RenderGBuffer.fx");
            }
                // copy the textures in the original material to the new normal mapping
                // material, if they are relevant to our renderer. The
                // LookUpTextures function has added the normal map and specular map
                // textures to the Textures collection, so that will be copied as well.
                foreach (KeyValuePair<String, ExternalReference<TextureContent>> texture
                in material.Textures)
                {
                    if ((texture.Key == "Texture") ||
                            (texture.Key == "NormalMap") ||
                            (texture.Key == "SpecularMap"))
                        deferredShadingMaterial.Textures.Add(texture.Key, texture.Value);
                }

                return context.Convert<MaterialContent, MaterialContent>(deferredShadingMaterial, typeof(MaterialProcessor).Name);
            }
         


    }
}