using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace pactheman_server {
    
    sealed class AStar {

        private static readonly Lazy<AStar> lazy = new Lazy<AStar>(() => new AStar());
        public static AStar Instance { get { return lazy.Value; } }
        private AStar() {}

        double ManhattanDistance(Vector2 start, Vector2 end) {
            return Math.Abs(start.X - end.X) + Math.Abs(start.Y - end.Y);
        }

        public List<Vector2> GetPath(Vector2 start, Vector2 end, int iterDepth = -3) {

            var maze = GameEnv.Instance.MapAsTiles;
            var startNode = new Node(null, start);
            var endNode = new Node(null, end);

            var openList = new List<Node>();
            var closedList = new List<Node>();

            openList.Add(startNode);

            var iteration = 0;

            while (openList.Count > 0) {

                var currentIndex = openList.Select(node => node.f).ToList().MinIndex();
                var currentNode = openList.Pop(currentIndex);
                closedList.Add(currentNode);

                // iterDepth + 2 cause start point gets removed
                if (iteration == iterDepth + 2 || currentNode == endNode) {
                    var path = new List<Vector2>();
                    var current = currentNode;
                    while (current != null) {
                        path.Add(current.Position);
                        current = current.Parent;
                    }
                    // return without start point
                    path.RemoveAt(path.Count - 1);
                    return path;
                }

                var children = new List<Node>();
                // adjacent squares -> left, right, top, bottom
                foreach (var newPosition in new List<Vector2>().AddMany(new Vector2(-1, 0), new Vector2(1, 0), new Vector2(0, 1), new Vector2(0, -1))) {

                    var nodePosition = new Vector2(
                        currentNode.Position.X + newPosition.X,
                        currentNode.Position.Y + newPosition.Y
                    );

                    if (nodePosition.X > (maze.GetLength(0) - 1) || nodePosition.X <= 0 // valid cause map has border
                        || nodePosition.Y > (maze.GetLength(1) - 1) || nodePosition.Y <= 0)
                        continue;

                    if (maze[(int) nodePosition.X, (int) nodePosition.Y] != 0) continue;

                    var newNode = new Node(currentNode, nodePosition);

                    if (!closedList.Contains(newNode)) children.Add(newNode);

                }

                foreach (var child in children) {

                    child.g = currentNode.g + 1;
                    child.h = ManhattanDistance(child.Position, endNode.Position);
                    child.f = child.g + child.h;

                    if (openList.Contains(child)) continue;

                    openList.Add(child);
                }

                iteration++;
            }
            return null;
        }

    }
}