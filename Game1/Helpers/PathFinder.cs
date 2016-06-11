﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Game1.Helpers
{
    class PathFinder
    {

        /*public struct Grid
        {
            public Rectangle Size;

            public byte[,] Weight;

            public Grid(int x, int y, byte defaultValue = 0)
            {
                Size = new Rectangle(0, 0, x, y);
                Weight = new byte[x, y];

                for (var i = 0; i < x; i++)
                {
                    for (var j = 0; j < y; j++)
                    {
                        Weight[i, j] = defaultValue;
                    }
                }
            }*/

        public float CubeDistance(CubeCoordinate a, CubeCoordinate b)
        {
            //return (Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y) + Math.Abs(a.Z - b.Z)) / 2;
            return Math.Max(Math.Max(Math.Abs(a.x - b.x), Math.Abs(a.y - b.y)), Math.Abs(a.z - b.z));
        }

        public List<CubeCoordinate> CubeLerp(CubeCoordinate a, CubeCoordinate b)
        {
            var ret = new List<CubeCoordinate>();
            var distance = CubeDistance(a, b);

            int steps = (int)distance;

            for (int i = 1; i <= steps; i++)
            {
                float amount = 1.0f / steps * i;
                ret.Add(new CubeCoordinate(MathHelper.Lerp(a.x, b.x, amount), MathHelper.Lerp(a.y, b.y, amount), MathHelper.Lerp(a.z, b.z, amount), MathHelper.Lerp(a.height, b.height, amount)));
            }

            return ret;
        }

        public List<Tile> Pathfind(Tile start, Tile end, Octree octree)
        {
            // nodes that have already been analyzed and have a path from the start to them
            var closedSet = new List<Tile>();
            // nodes that have been identified as a neighbor of an analyzed node, but have 
            // yet to be fully analyzed
            var openSet = new List<Tile> { start };
            // a dictionary identifying the optimal origin Tile to each node. this is used 
            // to back-track from the end to find the optimal path
            var cameFrom = new Dictionary<Tile, Tile>();
            // a dictionary indicating how far each analyzed node is from the start
            var currentDistance = new Dictionary<Tile, int>();
            // a dictionary indicating how far it is expected to reach the end, if the path 
            // travels through the specified node. 
            var predictedDistance = new Dictionary<Tile, float>();

            // initialize the start node as having a distance of 0, and an estmated distance 
            // of y-distance + x-distance, which is the optimal path in a square grid that 
            // doesn't allow for diagonal movement
            currentDistance.Add(start, 0);
            predictedDistance.Add(
                start,
                0 + +Math.Abs(start.Position.X - end.Position.X) + Math.Abs(start.Position.Z - end.Position.Z)
                + Math.Abs(start.Position.Y - end.Position.Y)
            );

            // if there are any unanalyzed nodes, process them
            while (openSet.Count > 0)
            {
                // get the node with the lowest estimated cost to finish
                var current = (
                    from p in openSet orderby predictedDistance[p] ascending select p
                ).First();

                // if it is the finish, return the path
                if (current.Position.X == end.Position.X && current.Position.Z == end.Position.Z && current.Position.Y == end.Position.Y)
                {
                    // generate the found path
                    return ReconstructPath(cameFrom, end);
                }

                // move current node from open to closed
                openSet.Remove(current);
                closedSet.Add(current);

                // process each valid node around the current node
                foreach (var neighbor in GetNeighborNodes(current, octree))
                {
                    var tempCurrentDistance = currentDistance[current] + 1;

                    // if we already know a faster way to this neighbor, use that route and 
                    // ignore this one
                    if (closedSet.Contains(neighbor)
                        && tempCurrentDistance >= currentDistance[neighbor])
                    {
                        continue;
                    }

                    // if we don't know a route to this neighbor, or if this is faster, 
                    // store this route
                    if (!closedSet.Contains(neighbor)
                        || tempCurrentDistance < currentDistance[neighbor])
                    {
                        if (cameFrom.Keys.Contains(neighbor))
                        {
                            cameFrom[neighbor] = current;
                        }
                        else
                        {
                            cameFrom.Add(neighbor, current);
                        }

                        currentDistance[neighbor] = tempCurrentDistance;
                        predictedDistance[neighbor] =
                            currentDistance[neighbor]
                            + Math.Abs(neighbor.Position.X - end.Position.X)
                            + Math.Abs(neighbor.Position.Z - end.Position.Z)
                            + Math.Abs(neighbor.Position.Y - current.Position.Y)*5;

                        // if this is a new node, add it to processing
                        if (!openSet.Contains(neighbor))
                        {
                            openSet.Add(neighbor);
                        }
                    }
                }
            }

            // unable to figure out a path, abort.
            throw new Exception(
                string.Format(
                    "unable to find a path between {0},{1} and {2},{3}",
                    start.Position.X, start.Position.Z,
                    end.Position.X, end.Position.Z
                )
            );
        }

        /// <summary>
        /// Return a list of accessible nodes neighboring a specified node
        /// </summary>
        /// <param name="node">The center node to be analyzed.</param>
        /// <returns>A list of nodes neighboring the node that are accessible.</returns>
        private IEnumerable<Tile> GetNeighborNodes(Tile node, Octree octree)
        {
            var nodes = new List<Tile>();
            List<DrawableObject> map = octree.AllObjects(DrawableObject.ObjectType.Terrain);
            BoundingSphere sphere = new BoundingSphere(node.Position, 30);
            List <IntersectionRecord> irs = octree.AllIntersections(sphere, DrawableObject.ObjectType.Terrain);

            List <IntersectionRecord> remove = new List <IntersectionRecord>();

            foreach (IntersectionRecord ir in irs)
            {
                if (ir.Position == node.Position || ir.Position.Y - node.Position.Y > 30 || node.Position.Y < 5)//usunięcie "siebie" z listy lub za wysokiego progu
                {
                    remove.Add(ir);
                }
            }

            foreach (IntersectionRecord ir in remove)
            {
                irs.Remove(ir);
            }


            foreach (IntersectionRecord ir in irs)
            {
                try { nodes.Add((Tile)ir.DrawableObjectObject); }//błedy przy typie DrawableObject
                catch { }
            }
            

            return nodes;
        }

        private IEnumerable<Tile> GetNeighborNodesSimple(Tile node, Octree octree)
        {
            var nodes = new List<Tile>();
            List<DrawableObject> map = octree.AllObjects(DrawableObject.ObjectType.Terrain);
            BoundingSphere sphere = new BoundingSphere(node.Position, 30);
            List<IntersectionRecord> irs = octree.AllIntersections(sphere, DrawableObject.ObjectType.Terrain);

            foreach (IntersectionRecord ir in irs)
            {
                if (ir.Position == node.Position)//usunięcie "siebie" z listy
                {
                    irs.Remove(ir);
                }
                else if (ir.Position.Y - node.Position.Y > 30)
                {
                    irs.Remove(ir);
                }
                else
                {
                    try { nodes.Add((Tile)ir.DrawableObjectObject); }//błedy przy typie DrawableObject
                    catch { }
                }
            }

            /*
            // up
            if (Weight[node.X, node.Y - 1] > 0)
            {
                nodes.Add(new Tile(node.X, node.Y - 1));
            }

            // right
            if (Weight[node.X + 1, node.Y] > 0)
            {
                nodes.Add(new Tile(node.X + 1, node.Y));
            }

            // down
            if (Weight[node.X, node.Y + 1] > 0)
            {
                nodes.Add(new Tile(node.X, node.Y + 1));
            }

            // left
            if (Weight[node.X - 1, node.Y] > 0)
            {
                nodes.Add(new Tile(node.X - 1, node.Y));
            }
            */
            return nodes;
        }

        /// <summary>
        /// Process a list of valid paths generated by the Pathfind function and return 
        /// a coherent path to current.
        /// </summary>
        /// <param name="cameFrom">A list of nodes and the origin to that node.</param>
        /// <param name="current">The destination node being sought out.</param>
        /// <returns>The shortest path from the start to the destination node.</returns>
        private List<Tile> ReconstructPath(Dictionary<Tile, Tile> cameFrom, Tile current)
        {
            if (!cameFrom.Keys.Contains(current))
            {
                return new List<Tile> { current };
            }

            var path = ReconstructPath(cameFrom, cameFrom[current]);
            path.Add(current);
            return path;
        }
    }

}
