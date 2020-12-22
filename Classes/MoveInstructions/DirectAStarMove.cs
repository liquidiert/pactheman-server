using System.Collections.Generic;
using PacTheMan.Models;

namespace pactheman_server {
    class DirectAStarMove : MoveInstruction {

        public DirectAStarMove(Actor moveable, Actor target) : base(moveable, target) {
            Moveable = moveable;
            Target = target;
        }

        public override List<Position> GetMoves(float elapsedSeconds, int iterDepth = 5) {
            return AStar.Instance.GetPath(Moveable.Position, Target.Position, iterDepth: iterDepth);
        }
    }
}