using System.Collections.Generic;
using PacTheMan.Models;

namespace pactheman_server {

    public abstract class MoveInstruction {

        public static readonly Dictionary<string, string> HumanReadableMoveInstructions = new Dictionary<string, string> {
            {"direct_astar", "Direct AStar"},
            {"patroling_astar", "Patroling AStar"},
            {"predicted_astar", "Predicted AStar"},
            {"random_astar", "Random AStar"},
        };

        public Actor Moveable;
        public Actor Target;

        public MoveInstruction(Actor moveable, Actor target) => (moveable, target) = (Moveable, Target);

        public abstract List<Position> GetMoves(float elapsedSeconds = 1/6, int iterDepth = 5);
    }
}