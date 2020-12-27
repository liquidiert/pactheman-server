using System.Collections.Generic;
using PacTheMan.Models;

namespace pactheman_server {
    class PredictedAStarMove : MoveInstruction {

        public override List<Position> GetMoves(Actor moveable, Actor target, float elapsedSeconds, int iterDepth = 3) {
            return AStar.Instance.GetPath(moveable.Position, target.FuturePosition(elapsedSeconds), iterDepth: iterDepth);
        }
    }
}