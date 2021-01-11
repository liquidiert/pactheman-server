using System.Collections.Generic;
using PacTheMan.Models;

namespace pactheman_server {
    class DirectAStarMove : MoveInstruction {

        public override List<Position> GetMoves(Actor moveable, Actor target, float elapsedSeconds, int iterDepth = 5) {
            return AStar.Instance.GetPath(moveable.DownScaledPosition, target.DownScaledPosition, iterDepth: iterDepth);
        }
    }
}