using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game1
{
    public class ObjectManager
    {
        private List<DrawableObject> list;
        private List<DrawableObject> toAdd;
        private List<DrawableObject> toRemove;

        public List<DrawableObject> List
        {
            get { return list; }
        }

        public ObjectManager()
        {
            list = new List<DrawableObject>();
            toAdd = new List<DrawableObject>();
            toRemove = new List<DrawableObject>();
        }

        public void Update(GameTime gameTime)
        {
            foreach(DrawableObject dObject in toRemove)
            {
                list.Remove(dObject);
            }
            list.AddRange(toAdd);

            toRemove.Clear();
            toAdd.Clear();

            foreach (DrawableObject dObject in list)
            {
                dObject.Update(gameTime);
            }
        }

        public void Draw(Camera camera)
        {
            foreach (DrawableObject dObject in list)
            {
                if (dObject.BoundingBox != null && dObject.BoundingBox.Max - dObject.BoundingBox.Min != Vector3.Zero)
                {
                    if (camera.Frustum.Contains(dObject.BoundingBox) != ContainmentType.Disjoint)
                        dObject.Draw(camera);
                }
                else if (dObject.BoundingSphere != null && dObject.BoundingSphere.Radius != 0f)
                {
                    if (camera.Frustum.Contains(dObject.BoundingSphere) != ContainmentType.Disjoint)
                        dObject.Draw(camera);
                }
            }
        }

        public void Add(DrawableObject dObject)
        {
            toAdd.Add(dObject);
        }

        public void Remove(DrawableObject dObject)
        {
            toRemove.Add(dObject);
        }

    }
}
