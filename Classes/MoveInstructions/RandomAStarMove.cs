using System.Collections.Generic;
using PacTheMan.Models;
using System;
using System.Linq;

namespace pactheman_server {
    class RandomAStarMove : MoveInstruction {

        public override List<Position> GetMoves(Actor moveable, Actor target, float elapsedSeconds, int iterDepth = 5) {
            var isCloseToCenter = moveable.Position.X > 2 || moveable.Position.X <= 20 && moveable.Position.Y > 2 || moveable.Position.Y <= 17;
            var possibleTargets = ((Tuple<Position, int>[,])Map.map.GetRegion(
                moveable.Position,
                regionSize: isCloseToCenter ? 5 : 3
            )).Where(t => t.Item2 == 0).Select(t => t.Item1).ToList();
            return AStar.Instance.GetPath(
                moveable.Position,
                possibleTargets[new Random().Next(possibleTargets.Count)],
                iterDepth: iterDepth
            );
        }
    }
}