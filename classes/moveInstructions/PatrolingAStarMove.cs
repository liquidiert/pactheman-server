using System.Collections.Generic;
using Microsoft.Xna.Framework;
using System;
using System.Linq;

namespace pactheman_server {
    public class PatrolingAStarMove : MoveInstruction {

        private Vector2 _randomPatrollingTarget(Actor target) {
            var targetPos = target.DownScaledPosition;
            var possibleTargets = ((Tuple<Vector2, int>[,])GameEnv.Instance.MapAsTiles.GetRegion(
                targetPos,
                regionSize: 3))
                    .Where(t => t.Item2 == 0).Select(t => t.Item1).ToList();
            return possibleTargets[new Random().Next(possibleTargets.Count)];
        }

        public override List<Vector2> GetMoves(Actor moveable, Actor target, float elapsedSeconds, int iterDepth = 5) {
            return AStar.Instance.GetPath(moveable.DownScaledPosition, _randomPatrollingTarget(target), iterDepth: iterDepth);
        }
    }
}