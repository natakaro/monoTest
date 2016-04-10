using Microsoft.Xna.Framework;
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
        public enum ObjectType
        {
            Unknown = 1,
            Terrain = 2,
            Unit = 4,
            Resource = 8,
            Item = 16,
            Projectile = 32,
            Ethereal = 64,      //stuff can go through this, not affected by most forces such as gravity
            ALL = Unknown | Terrain | Unit | Resource | Item | Projectile | Ethereal
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

        protected bool m_static = true;
        protected bool hasBounds = false;
        protected bool selected = false;
        protected int m_lod = 0;

        public DrawableObject(Game game, Matrix inWorldMatrix) : base(game)
        {
            type = ObjectType.Unknown;
            worldMatrix = inWorldMatrix;
            position = new Vector3(inWorldMatrix.M41, inWorldMatrix.M42, inWorldMatrix.M43);
            lastPosition = position;
            orientation = Quaternion.Identity;
            velocity = Vector3.Zero;
            acceleration = Vector3.Zero;
            boundingSphere = new BoundingSphere();
            boundingBox = new BoundingBox();
        }

        /// <summary>
        /// Moves an object according to its position, velocity and accelleration and the change in game time
        /// </summary>
        /// <param name="gameTime">The change in game time</param>
        /// <returns>True - the object was moved.
        /// False - The object did not move.</returns>
        public virtual bool Update(GameTime gameTime)
        {
            if (!m_static)
            {
                lastPosition = position;
                velocity += acceleration * (float)(gameTime.ElapsedGameTime.TotalSeconds);
                position += velocity * (float)(gameTime.ElapsedGameTime.TotalSeconds);
                boundingSphere.Center = position;

                return lastPosition != position;    //lets you know if the object actually moved relative to its last position
            }

            return false;
        }

        public abstract void Draw(Camera camera);

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

        public virtual IntersectionRecord Intersects(Ray intersectionRay)
        {
            return new IntersectionRecord(this);
        }

        public virtual Matrix Projection
        {
            set { effect.Parameters["xProjection"].SetValue(value); }
        }
        public virtual Matrix View
        {
            set { effect.Parameters["xView"].SetValue(value); }
        }

        public virtual void SetDirectionalLight(Vector3 direction, Color color)
        {
            effect.Parameters["xLightDirection0"].SetValue(direction);
            effect.Parameters["xLightColor0"].SetValue(color.ToVector3());
            effect.Parameters["xEnableLighting"].SetValue(true);
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
                ContainmentType ct = boundingBox.Contains(intersectionBox);
                if (ct != ContainmentType.Disjoint)
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

        public bool IsStatic
        {
            get { return m_static; }
            set { m_static = value; }
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
        #endregion
    }
}
