﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Simple.CGE.Helpers
{
    /// <summary>
    /// A* -based path solver
    /// </summary>
    public class PathSearch
    {
        private readonly Size mapSize;

        public int HVCost { get; set; } = 10;
        public int DCost { get; set; } = 14;
        public int DistanceCost { get; set; } = 15;


        public SearchNode[,] Map { get; }
        public Point StartPoint { get; }
        public Point FinishPoint { get; }
        public bool Finished { get; private set; }

        public PathSearch(Size mapSize, Point start, Point finish )
        {
            if (mapSize.Height <= 1) throw new ArgumentException("Map Height should be greater than 1");
            if (mapSize.Width <= 1) throw new ArgumentException("Map Width should be greater than 1");

            var rect = new Rectangle(new Point(), mapSize);
            if(!start.IsInside(rect)) throw new ArgumentException("Start point should be inside the map");
            if(!finish.IsInside(rect)) throw new ArgumentException("Finish point should be inside the map");

            Map = new SearchNode[mapSize.Width, mapSize.Height];
            this.mapSize = mapSize;
            StartPoint = start;
            FinishPoint = finish;

        }

        public void SetWalkable(char blocked, string walkableMap)
        {
            if (walkableMap.Length != Map.Length) throw new ArgumentException("The map should be equal sized");

            for (int x = 0; x < mapSize.Width; x++)
            {
                for (int y = 0; y < mapSize.Height; y++)
                {
                    int sPos = x + y * mapSize.Width;
                    Map[x, y].Obstruction = walkableMap[sPos] == blocked;
                }
            }
        }
        public void SetWalkable(bool[,] walkableMap)
        {
            if (walkableMap.Length != Map.Length) throw new ArgumentException("The map should be equal sized");

            for (int x = 0; x < mapSize.Width; x++)
            {
                for (int y = 0; y < mapSize.Height; y++)
                {
                    Map[x, y].Obstruction = !walkableMap[x, y];
                }
            }
        }

        public void SetWalkable(IEnumerable< Point> points)
        {
            foreach (var p in points) SetWalkable(p);
        }
        public void SetWalkable(Point point)
        {
            Map[point.X, point.Y].Obstruction = false;
        }
        public void SetNotWalkable(IEnumerable<Point> points)
        {
            foreach (var p in points) SetNotWalkable(p);
        }
        public void SetNotWalkable(Point point)
        {
            Map[point.X, point.Y].Obstruction = true;
        }

        /// <summary>
        /// Starts or restarts the search process
        /// </summary>
        public void Setup()
        {
            for (int x = 0; x < mapSize.Width; x++)
            {
                for (int y = 0; y < mapSize.Height; y++)
                {
                    var n = new SearchNode();

                    n.Coordinates = new Point(x, y);
                    n.IsClosedSet = false;
                    n.IsOpenSet = false;
                    n.IsNullSet = true;
                    n.ParentNode = null;
                    n.GCost = int.MaxValue;
                    //n.HCost = int.MaxValue;
                    n.UpdateHCost(DistanceCost, FinishPoint);

                    Map[x, y] = n;
                }
            }

            var startNode = Map[StartPoint.X, StartPoint.Y];
            startNode.IsOpenSet = true;
            startNode.GCost = 0;
            startNode.UpdateHCost(DistanceCost, FinishPoint);

            Finished = false;
        }
        /// <summary>
        /// Executes one step on the process
        /// </summary>
        /// <returns>True if there are no more paths to search, else if there is</returns>
        public bool DoStep()
        {
            if (Finished) return true;

            // get Smaller OpenNode
            var item = Map.Cast<SearchNode>()
                          .Where(n => n.IsOpenSet)
                          .OrderBy(n => n.FCost)
                          .FirstOrDefault();

            if (item == null) return true; // finished

            if (item.Coordinates == FinishPoint)
            {
                Finished = true;

                var temp = item;
                while (temp.ParentNode != null)
                {
                    temp.IsFinishedPath = true;
                    temp = temp.ParentNode;
                }

                return false;
            }

            // check neighbors
            tryCheckNeighbor(item, item.Coordinates.X - 1, item.Coordinates.Y - 1, isDiagonal: true);
            tryCheckNeighbor(item, item.Coordinates.X, item.Coordinates.Y - 1, isDiagonal: false);
            tryCheckNeighbor(item, item.Coordinates.X + 1, item.Coordinates.Y - 1, isDiagonal: true);

            tryCheckNeighbor(item, item.Coordinates.X - 1, item.Coordinates.Y, isDiagonal: false);
            tryCheckNeighbor(item, item.Coordinates.X + 1, item.Coordinates.Y, isDiagonal: false);

            tryCheckNeighbor(item, item.Coordinates.X - 1, item.Coordinates.Y + 1, isDiagonal: true);
            tryCheckNeighbor(item, item.Coordinates.X, item.Coordinates.Y + 1, isDiagonal: false);
            tryCheckNeighbor(item, item.Coordinates.X + 1, item.Coordinates.Y + 1, isDiagonal: true); ;
            // not open anymore
            item.IsOpenSet = false;
            item.IsClosedSet = true;

            return false;
        }
        private void tryCheckNeighbor(SearchNode parent, int x, int y, bool isDiagonal)
        {
            if (x < 0) return;
            if (y < 0) return;
            if (x >= mapSize.Width) return;
            if (y >= mapSize.Height) return;

            var node = Map[x, y];
            if (node.Obstruction) return;
            if (node.IsOpenSet) return;
            if (node.IsClosedSet) return;

            // set as Open
            node.IsOpenSet = true;
            node.IsNullSet = false;
            node.ParentNode = parent;
            node.GCost = parent.GCost + (isDiagonal ? DCost : HVCost);
        }

        public class SearchNode
        {
            public int NodeNumber { get; set; }
            public Point Coordinates { get; set; }
            /// <summary>
            /// Cost from start
            /// </summary>
            public int GCost { get; set; }
            /// <summary>
            /// Cost from End
            /// </summary>
            public int HCost { get; set; }
            public int FCost => HCost + GCost;

            public SearchNode ParentNode { get; set; }
            public bool Obstruction { get; set; }
            /// <summary>
            /// Nor seen nor visited
            /// </summary>
            public bool IsNullSet { get; set; }
            /// <summary>
            /// Seen and visited
            /// </summary>
            public bool IsClosedSet { get; set; }
            /// <summary>
            /// Seen but not visited
            /// </summary>
            public bool IsOpenSet { get; set; }

            public bool IsFinishedPath { get; set; }

            public void UpdateHCost(int weight, Point finishPoint)
            {
                HCost = (int)(weight * EntityHelper.Distance(Coordinates, finishPoint));
            }
        }
    }
}