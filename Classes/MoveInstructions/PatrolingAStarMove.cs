using System.Collections.Generic;
using System;
using System.Linq;
using PacTheMan.Models;

namespace pactheman_server {
    public class PatrolingAStarMove : MoveInstruction {

        private Position _randomPatrollingTarget(Actor target) {
            var targetPos = target.DownScaledPosition;
            var possibleTargets = ((Tuple<Position, int>[,])Map.map.GetRegion(
                targetPos,
                regionSize: 3))
                    .Where(t => t.Item2 == 0).Select(t => t.Item1).ToList();
            return possibleTargets[new Random().Next(possibleTargets.Count)];
        }

        public override List<Position> GetMoves(Actor moveable, Actor target, float elapsedSeconds, int iterDepth = 5) {
            return AStar.Instance.GetPath(moveable.DownScaledPosition, _randomPatrollingTarget(target), iterDepth: iterDepth);
        }
    }
}