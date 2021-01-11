using System.Collections.Generic;
using PacTheMan.Models;
using System;
using System.Linq;

namespace pactheman_server {
    class RandomAStarMove : MoveInstruction {

        public override List<Position> GetMoves(Actor moveable, Actor target, float elapsedSeconds, int iterDepth = 5) {
            var isCloseToCenter = moveable.DownScaledPosition.X > 2 || moveable.DownScaledPosition.X <= 20 &&
                moveable.DownScaledPosition.Y > 2 || moveable.DownScaledPosition.Y <= 17;
            var possibleTargets = ((Tuple<Position, int>[,])Map.map.GetRegion(
                moveable.DownScaledPosition,
                regionSize: isCloseToCenter ? 5 : 3
            )).Where(t => t.Item2 == 0).Select(t => t.Item1).ToList();
            return AStar.Instance.GetPath(
                moveable.DownScaledPosition,
                possibleTargets[new Random().Next(possibleTargets.Count)],
                iterDepth: iterDepth
            );
        }
    }
}