using System.Collections.Generic;
using PacTheMan.Models;
using System;

namespace pactheman_server {

    public abstract class MoveInstruction {

        public static MoveInstruction FromString(String instruction) {
            switch (instruction) {
                case "direct_astar":
                    return new DirectAStarMove();
                case "patroling_astar":
                    return new PatrolingAStarMove();
                case "predicted_astar":
                    return new PredictedAStarMove();
                case "random_astar":
                    return new RandomAStarMove();
                default:
                    throw new Exception("Unknown moving instruction");
            }
        }

        public abstract List<Position> GetMoves(Actor moveable, Actor target, float elapsedSeconds = 1/6, int iterDepth = 5);
    }
}