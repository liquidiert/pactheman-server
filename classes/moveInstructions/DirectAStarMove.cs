using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace pactheman_server {
    class DirectAStarMove : MoveInstruction {

        public override List<Vector2> GetMoves(Actor moveable, Actor target, float elapsedSeconds, int iterDepth = 5) {
            return AStar.Instance.GetPath(moveable.DownScaledPosition, target.DownScaledPosition, iterDepth: iterDepth);
        }
    }
}