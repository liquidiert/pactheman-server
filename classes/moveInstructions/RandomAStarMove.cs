using System.Collections.Generic;
using Microsoft.Xna.Framework;
using System;
using System.Linq;

namespace pactheman_server {
    class RandomAStarMove : MoveInstruction {
        public override List<Vector2> GetMoves(Actor moveable, Actor target, float elapsedSeconds, int iterDepth = 5) {
            var isCloseToCenter = moveable.DownScaledPosition.X > 2 || moveable.DownScaledPosition.X <= 20 && moveable.DownScaledPosition.Y > 2 || moveable.DownScaledPosition.Y <= 17;
            var possibleTargets = ((Tuple<Vector2, int>[,]) GameEnv.Instance.MapAsTiles.GetRegion(
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