using System.Collections.Generic;
using PacTheMan.Models;

namespace pactheman_server {
    class PredictedAStarMove : MoveInstruction {

        public PredictedAStarMove(Actor moveable, Actor target) : base(moveable, target) {}

        public override List<Position> GetMoves(float elapsedSeconds, int iterDepth = 3) {
            return AStar.Instance.GetPath(Moveable.Position, Target.FuturePosition(elapsedSeconds), iterDepth: iterDepth);
        }
    }
}