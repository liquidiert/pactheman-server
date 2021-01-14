using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace pactheman_server {
    class PredictedAStarMove : MoveInstruction {
        public override List<Vector2> GetMoves(Actor moveable, Actor target, float elapsedSeconds, int iterDepth = 3) {
            return AStar.Instance.GetPath(
                moveable.DownScaledPosition, target.FuturePosition(elapsedSeconds).DivideValue(64).FloorInstance(), iterDepth: iterDepth);
        }
    }
}