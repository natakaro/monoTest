using Game1.Helpers;
using Game1.Screens;
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
    public abstract class DrawableObject : DrawableGameComponent
    {
        [Flags]
        public enum ObjectType
        {
            Player = 1,
            Tile = 2,
            Core = 4,
            Turret = 8,
            Enemy = 16,
            Spawn = 32,
            Projectile = 64,
            Item = 128,
            Asset = 256,
            Ethereal = 512,      //stuff can go through this, not affected by most forces such as gravity
            ALL = Player | Tile | Core | Turret | Enemy | Spawn | Projectile | Item | Asset | Ethereal
        };

        /// <summary>
        /// This is the broad phase bounding sphere. Use this first for any collision detection since it is the fastest to calculate.
        /// Note: Use length squared to avoid a square root calculation!
        /// </summary>
        protected BoundingSphere boundingSphere;

        /// <summary>
        /// this is a coarse bounding box for the entire object. If this bounding box doesn't intersect with another coarse bounding box, 
        /// then there isn't a need for any further collision checks.
        /// </summary>
        protected BoundingBox boundingBox;

        protected Matrix worldMatrix;

        /// <summary>
        /// This indicates that the object doesn't actually move (such as terrain)
        /// </summary>
        
        protected Vector3 position;
        protected Vector3 lastPosition;
        protected Quaternion orientation;
        protected Vector3 velocity;
        protected Vector3 acceleration;
        protected ObjectType type;
        protected Effect effect;
        protected Model model;
        protected Matrix[] modelBones;
        protected Octree octree;

        protected bool m_static = true;
        protected bool hasBounds = false;
        protected bool selected = false;
        protected bool alive = true;
        protected int m_lod = 0;

        protected bool m_instanced = false;

        protected float scale;

        protected float dissolveAmount = 0;

        public DrawableObject(Game game, Matrix inWorldMatrix, Model inModel, Octree octree) : base(game)
        {
            type = ObjectType.Player;
            worldMatrix = inWorldMatrix;
            position = new Vector3(inWorldMatrix.M41, inWorldMatrix.M42, inWorldMatrix.M43);
            lastPosition = position;
            orientation = worldMatrix.Rotation;
            velocity = Vector3.Zero;
            acceleration = Vector3.Zero;
            boundingSphere = new BoundingSphere();
            boundingBox = new BoundingBox();
            model = inModel;
            modelBones = new Matrix[model.Bones.Count];
            model.CopyAbsoluteBoneTransformsTo(modelBones);

            scale = 1;

            this.octree = octree;
        }

        /// <summary>
        /// Moves an object according to its position, velocity and accelleration and the change in game time
        /// </summary>
        /// <param name="gameTime">The change in game time</param>
        /// <returns>True - the object was moved.
        /// False - The object did not move.</returns>
        public new virtual bool Update(GameTime gameTime)
        {
            if (!m_static)
            {
                lastPosition = position;
                velocity += acceleration * (float)(gameTime.ElapsedGameTime.TotalSeconds);
                position += velocity * (float)(gameTime.ElapsedGameTime.TotalSeconds);

                boundingSphere.Center = position;
                boundingBox.Min += velocity * (float)(gameTime.ElapsedGameTime.TotalSeconds);
                boundingBox.Max += velocity * (float)(gameTime.ElapsedGameTime.TotalSeconds);

                worldMatrix = Matrix.CreateTranslation(position);
                if (lastPosition != position)
                    CheckIntersections();
                return lastPosition != position;    //lets you know if the object actually moved relative to its last position
            }

            return false;
        }

        public void CheckIntersections()
        {
            //List<IntersectionRecord> list = new List<IntersectionRecord>();
            List<IntersectionRecord> list = octree.AllIntersections(this);
            //if (boundingSphere != null && boundingSphere.Radius != 0f)
            //{
            //    list = octree.AllIntersections(boundingSphere);
            //}
            //else if (boundingBox != null && boundingBox.Max != boundingBox.Min)
            //{
            //    list = octree.AllIntersections(boundingBox);
            //}
            //else
            //    list = new List<IntersectionRecord>();
            //List<DrawableObject> temp = new List<DrawableObject>();
            //temp.Add(this);
            //list = octree.GetIntersection(temp);

            foreach (IntersectionRecord ir in list)
            {
                if (ir.DrawableObjectObject != null)
                    ir.DrawableObjectObject.HandleIntersection(ir);
                if (ir.OtherDrawableObjectObject != null)
                    ir.OtherDrawableObjectObject.HandleIntersection(ir);
            }
        }

        public virtual void Draw(Camera camera)
        {
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (Effect effect in mesh.Effects)
                {
                    effect.CurrentTechnique = effect.Techniques["Technique1"];
                    effect.Parameters["World"].SetValue(Matrix.CreateScale(scale) * modelBones[mesh.ParentBone.Index] * worldMatrix);
                    effect.Parameters["View"].SetValue(camera.ViewMatrix);
                    effect.Parameters["Projection"].SetValue(camera.ProjectionMatrix);
                    effect.Parameters["FarClip"].SetValue(camera.FarZ);
                    effect.Parameters["Clipping"].SetValue(false);
                }
                mesh.Draw();
            }
        }

        public virtual void Draw(Camera camera, Matrix viewMatrix, Vector4 clipPlane)
        { 
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (Effect effect in mesh.Effects)
                {
                    effect.Parameters["World"].SetValue(Matrix.CreateScale(scale) * modelBones[mesh.ParentBone.Index] * worldMatrix);
                    effect.Parameters["View"].SetValue(viewMatrix);
                    effect.Parameters["Projection"].SetValue(camera.ProjectionMatrix);
                    effect.Parameters["FarClip"].SetValue(camera.FarZ);
                    effect.Parameters["Clipping"].SetValue(true);
                    effect.Parameters["ClipPlane"].SetValue(clipPlane);
                    //effect.Parameters["DissolveMap"].SetValue(GameplayScreen.assetContentContainer.dissolveTexture);
                    //effect.Parameters["DissolveThreshold"].SetValue(dissolveAmount);
                    //effect.Parameters["EdgeMap"].SetValue(GameplayScreen.assetContentContainer.edgeTexture);
                }
                mesh.Draw();
            }
        }

        public virtual void UpdateLOD(Camera camera)
        {
            float dist = (camera.Position - position).LengthSquared();

            if (dist <= 2500)
                m_lod = 0;
            else if (dist <= 10000)
                m_lod = 1;
            else
                m_lod = 2;

        }

        /// <summary>
        /// Tells you if the bounding regions for this object [intersect or are contained within] the bounding frustum
        /// </summary>
        /// <param name="intersectionFrustum">The frustum to do bounds checking against</param>
        /// <returns>An intersection record containing any intersection information, or null if there isn't any
        /// </returns>
        public virtual IntersectionRecord Intersects(BoundingFrustum intersectionFrustum)
        {

            if (boundingBox != null && boundingBox.Max - boundingBox.Min != Vector3.Zero)
            {
                if (intersectionFrustum.Contains(boundingBox) != ContainmentType.Disjoint)
                    return new IntersectionRecord(this);
            }
            else if (boundingSphere != null && boundingSphere.Radius != 0f)
            {
                if (intersectionFrustum.Contains(boundingSphere) != ContainmentType.Disjoint)
                    return new IntersectionRecord(this);
            }

            //if (boundingBox != null && boundingBox.Max - boundingBox.Min != Vector3.Zero)
            //{
            //    if (intersectionFrustum.FastIntersectTest(ref boundingBox))
            //        return new IntersectionRecord(this);
            //}
            //else if (boundingSphere != null && boundingSphere.Radius != 0f)
            //{
            //    if (intersectionFrustum.FastIntersectTest(ref boundingSphere))
            //        return new IntersectionRecord(this);
            //}

            return null;
        }

        /// <summary>
        /// Coarse collision check: Tells you if this object intersects with the given intersection sphere.
        /// </summary>
        /// <param name="intersectionSphere">The intersection sphere to check against</param>
        /// <returns>An intersection record containing this object</returns>
        /// <remarks>You'll want to override this for granular collision detection</remarks>
        public virtual IntersectionRecord Intersects(BoundingSphere intersectionSphere)
        {
            if (boundingBox != null && boundingBox.Max != boundingBox.Min)
            {
                if (boundingBox.Contains(intersectionSphere) != ContainmentType.Disjoint)
                    return new IntersectionRecord(this);
            }
            else if (boundingSphere != null && boundingSphere.Radius != 0f)
            {
                if (boundingSphere.Contains(intersectionSphere) != ContainmentType.Disjoint)
                    return new IntersectionRecord(this);
            }

            return null;
        }

        /// <summary>
        /// Coarse collision check: Tells you if this object intersects with the given intersection box.
        /// </summary>
        /// <param name="intersectionBox">The intersection box to check against</param>
        /// <returns>An intersection record containing this object</returns>
        /// <remarks>You'll want to override this for granular collision detection</remarks>
        public virtual IntersectionRecord Intersects(BoundingBox intersectionBox)
        {
            if (boundingBox != null && boundingBox.Max != boundingBox.Min)
            {
                if (boundingBox.Contains(intersectionBox) != ContainmentType.Disjoint)
                    return new IntersectionRecord(this);
            }
            else if (boundingSphere != null && boundingSphere.Radius != 0f)
            {
                if (boundingSphere.Contains(intersectionBox) != ContainmentType.Disjoint)
                    return new IntersectionRecord(this);
            }

            return null;
        }

        /// <summary>
        /// Tests for intersection with this object against the other object
        /// </summary>
        /// <param name="otherObj">The other object to test for intersection against</param>
        /// <returns>Null if there isn't an intersection, an intersection record if there is a hit.</returns>
        public virtual IntersectionRecord Intersects(DrawableObject otherObj)
        {
            IntersectionRecord ir;

            if (otherObj.boundingBox != null && otherObj.boundingBox.Min != otherObj.boundingBox.Max)
            {
                ir = Intersects(otherObj.boundingBox);
            }
            else if (otherObj.boundingSphere != null && otherObj.boundingSphere.Radius != 0f)
            {
                ir = Intersects(otherObj.boundingSphere);
            }
            else
                return null;

            if (ir != null)
            {
                ir.DrawableObjectObject = this;
                ir.OtherDrawableObjectObject = otherObj;
            }

            return ir;
        }

        public virtual void HandleIntersection(IntersectionRecord ir)
        {

        }

        #region helper functions
        public void UndoLastMove()
        {
            position = lastPosition;
        }

        public void SetCollisionRadius(float radius)
        {
            boundingSphere.Radius = radius;
        }
        #endregion

        #region Accessors
        public ObjectType Type { get { return type; } }

        public Vector3 Position
        {
            get
            {
                return position;
            }
            set
            {
                position = value;
                boundingSphere.Center = value;
            }
        }

        /// <summary>
        /// Indicates whether or not this object has been selected by a player.
        /// </summary>
        public bool Selected
        {
            get { return selected; }
            set { selected = value; }
        }

        public bool Alive
        {
            get { return alive; }
            set { alive = value; }
        }

        public bool IsStatic
        {
            get { return m_static; }
            set { m_static = value; }
        }
        public bool IsInstanced
        {
            get { return m_instanced; }
            set { m_instanced = value; }
        }
        public BoundingBox BoundingBox
        {
            get
            {
                return boundingBox;
            }
            set
            {
                boundingBox = value;
            }
        }

        public BoundingSphere BoundingSphere
        {
            get
            {
                return boundingSphere;
            }
            set
            {
                boundingSphere = value;
            }
        }

        public Quaternion Orientation
        {
            get
            {
                return orientation;
            }
            set
            {
                orientation = value;
            }
        }

        public Vector3 Velocity
        {
            get
            {
                return velocity;
            }
            set
            {
                velocity = value;
            }
        }

        public Vector3 Acceleration
        {
            get { return acceleration; }
            set { acceleration = value; }
        }

        public float SpeedSquared
        {
            get { return velocity.LengthSquared(); }
        }

        public float Speed
        {
            get { return velocity.Length(); }
            set
            {
                velocity.Normalize();
                velocity *= value;
            }
        }

        public float Scale
        {
            get { return scale; }
            set { scale = value; }
        }

        /// <summary>
        /// tells you if a valid bounding area encloses the object. Doesn't indicate which kind though.
        /// </summary>
        public bool HasBounds
        {
            get
            {
                return (boundingSphere.Radius != 0 || boundingBox.Min != boundingBox.Max);
            }
        }

        public Effect Effect
        {
            get { return effect; }
            set { effect = value; }
        }

        public Matrix WorldMatrix
        {
            get
            {
                return worldMatrix;
            }
            set
            {
                worldMatrix = value;
                position = new Vector3(value.M41, value.M42, value.M43);
            }
        }

        public Model Model
        {
            get { return model; }
            set { model = value; }
        }

        public Matrix[] ModelBones
        {
            get { return modelBones; }
        }
        #endregion
    }
}
