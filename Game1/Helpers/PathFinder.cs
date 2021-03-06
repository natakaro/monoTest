﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using static Game1.Helpers.HexCoordinates;
using Game1.Screens;
using Game1.Lights;

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

        public List<Tile> Pathfind(Tile start, Tile end, Octree octree, GameSettings settings)
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

            var straightLine = CubeLerp(pixelToAxialH(start.Position, Map.size).ToCube(), pixelToAxialH(end.Position, Map.size).ToCube());

            // initialize the start node as having a distance of 0, and an estmated distance 
            // of y-distance + x-distance, which is the optimal path in a square grid that 
            // doesn't allow for diagonal movement
            //currentDistance.Add(start, 0);
            //predictedDistance.Add(
            //    start,
            //    0 + +Math.Abs(start.Position.X - end.Position.X) + Math.Abs(start.Position.Z - end.Position.Z)
            //    + Math.Abs(start.Position.Y - end.Position.Y)
            //);

            currentDistance.Add(start, 0);
            predictedDistance.Add(
                start,
                CubeDistance(pixelToAxialH(start.Position, Map.size).ToCube(), pixelToAxialH(end.Position, Map.size).ToCube())
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

                        
                        var distanceToStraightLine = DistanceToStraightLine(neighbor, straightLine);

                        currentDistance[neighbor] = tempCurrentDistance;
                        predictedDistance[neighbor] =
                            currentDistance[neighbor]
                            + Math.Abs(neighbor.Position.X - end.Position.X)
                            + Math.Abs(neighbor.Position.Z - end.Position.Z)
                            + Math.Abs(neighbor.Position.Y - current.Position.Y)
                            + distanceToStraightLine;
                        

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

        public List<Tile> Pathfind(Tile start, Tile end, Dictionary<AxialCoordinate, Tile> map, bool settings)
        {
            if (start != null && end != null)
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

                var straightLine = CubeLerp(pixelToAxialH(start.Position, Map.size).ToCube(), pixelToAxialH(end.Position, Map.size).ToCube());

                // initialize the start node as having a distance of 0, and an estmated distance 
                // of y-distance + x-distance, which is the optimal path in a square grid that 
                // doesn't allow for diagonal movement
                //currentDistance.Add(start, 0);
                //predictedDistance.Add(
                //    start,
                //    0 + +Math.Abs(start.Position.X - end.Position.X) + Math.Abs(start.Position.Z - end.Position.Z)
                //    + Math.Abs(start.Position.Y - end.Position.Y)
                //);

                currentDistance.Add(start, 0);
                predictedDistance.Add(
                    start,
                    CubeDistance(pixelToAxialH(start.Position, Map.size).ToCube(), pixelToAxialH(end.Position, Map.size).ToCube())
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
                        var path = ReconstructPath(cameFrom, end);
                        foreach (Tile tile in path)
                        {
                            tile.IsPath = true;
                        }
                        return path;
                    }

                    // move current node from open to closed
                    openSet.Remove(current);
                    closedSet.Add(current);

                    // process each valid node around the current node
                    foreach (var neighbor in GetNeighborNodes(current, map))
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

                            if (settings)
                            {
                                var distanceToStraightLine = DistanceToStraightLine(neighbor, straightLine);

                                currentDistance[neighbor] = tempCurrentDistance;
                                predictedDistance[neighbor] =
                                    currentDistance[neighbor]
                                    + Math.Abs(neighbor.Position.X - end.Position.X)
                                    + Math.Abs(neighbor.Position.Z - end.Position.Z)
                                    + Math.Abs(neighbor.Position.Y - current.Position.Y)*5
                                    +distanceToStraightLine;
                            }
                            else
                            {
                                currentDistance[neighbor] = tempCurrentDistance;
                                predictedDistance[neighbor] =
                                    currentDistance[neighbor]
                                    + Math.Abs(neighbor.Position.X - end.Position.X)
                                    + Math.Abs(neighbor.Position.Z - end.Position.Z)
                                    + Math.Abs(neighbor.Position.Y - current.Position.Y)*5;
                            }


                            // if this is a new node, add it to processing
                            if (!openSet.Contains(neighbor))
                            {
                                openSet.Add(neighbor);
                            }
                        }
                    }
                }

                // unable to figure out a path, abort.
                //throw new Exception(
                //    string.Format(
                //        "unable to find a path between {0},{1} and {2},{3}",
                //        start.Position.X, start.Position.Z,
                //        end.Position.X, end.Position.Z
                //    )
                //);
                return null;
            }
            else
                return new List<Tile>();
        }

        public List<Vector3> PathfindMiddle(List<Tile> tileList)
        {
            List<Vector3> ret = new List<Vector3>();
            Tile previous = tileList[0];


            foreach (Tile current in tileList)
            {
                ret.Add(current.Position);
            }
            /*
            foreach (Tile current in tileList)
            {
                Vector3 distance = (current.Position - previous.Position) / 2;
                ret.Add(previous.Position + distance);
                previous = current;
            }*/

            return ret;
        }

        private float DistanceToStraightLine(Tile tile, List<CubeCoordinateH> line)
        {
            var tileCubeCoord = pixelToAxialH(tile.Position, Map.size).ToCube();

            float distance = float.MaxValue;

            foreach(CubeCoordinateH coord in line)
            {
                var checkDistance = CubeDistance(tileCubeCoord, coord);
                if (checkDistance < distance)
                    distance = checkDistance;
            }
            distance *= Map.size;
            return distance;
        }

        /// <summary>
        /// Return a list of accessible nodes neighboring a specified node
        /// </summary>
        /// <param name="node">The center node to be analyzed.</param>
        /// <returns>A list of nodes neighboring the node that are accessible.</returns>
        private IEnumerable<Tile> GetNeighborNodes(Tile node, Octree octree)
        {
            var nodes = new List<Tile>();
            List<DrawableObject> map = octree.AllObjects(DrawableObject.ObjectType.Tile);
            BoundingSphere sphere = new BoundingSphere(node.Position, 30);
            List <IntersectionRecord> irs = octree.AllIntersections(sphere, DrawableObject.ObjectType.Tile);

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
            List<DrawableObject> map = octree.AllObjects(DrawableObject.ObjectType.Tile);
            BoundingSphere sphere = new BoundingSphere(node.Position, 30);
            List<IntersectionRecord> irs = octree.AllIntersections(sphere, DrawableObject.ObjectType.Tile);

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
        /// Return a list of accessible nodes neighboring a specified node
        /// </summary>
        /// <param name="node">The center node to be analyzed.</param>
        /// <returns>A list of nodes neighboring the node that are accessible.</returns>
        private IEnumerable<Tile> GetNeighborNodes(Tile node, Dictionary<AxialCoordinate, Tile> map)
        {
            var neighbors = new List<Tile>();

            CubeCoordinate coords = pixelToAxialH(node.Position, Map.size).ToCube();
            coords = CubeRound(coords);

            CubeCoordinate[] directions = 
            {
                new CubeCoordinate(+1, -1, 0), new CubeCoordinate(+1, 0, -1), new CubeCoordinate(0, +1, -1),
                new CubeCoordinate(-1, +1, 0), new CubeCoordinate(-1, 0, +1), new CubeCoordinate(0, -1, +1)
            };

            for(int i = 0; i < 6; i++)
            {
                CubeCoordinate neighborcoords = coords + directions[i];
                Tile tile = tileFromAxial(neighborcoords.ToAxial(), map);
                if (tile == null || tile.Position.Y - node.Position.Y > 20 || tile.Position.Y < 5 || tile.ObjectOn != null)
                    continue;
                else
                    neighbors.Add(tile);
            }

            return neighbors;
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

