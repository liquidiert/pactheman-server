using System.Collections.Generic;
using PacTheMan.Models;
using System;
using System.Linq;

namespace pactheman_server {
    class RandomAStarMove : MoveInstruction {

        public RandomAStarMove(Actor moveable, Actor target) : base(moveable, target) { }

        public override List<Position> GetMoves(float elapsedSeconds, int iterDepth = 5) {
            var isCloseToCenter = Moveable.Position.X > 2 || Moveable.Position.X <= 20 && Moveable.Position.Y > 2 || Moveable.Position.Y <= 17;
            var possibleTargets = ((Tuple<Position, int>[,])Map.map.GetRegion(
                Moveable.Position,
                regionSize: isCloseToCenter ? 5 : 3
            )).Where(t => t.Item2 == 0).Select(t => t.Item1).ToList();
            return AStar.Instance.GetPath(
                Moveable.Position,
                possibleTargets[new Random().Next(possibleTargets.Count)],
                iterDepth: iterDepth
            );
        }
    }
}