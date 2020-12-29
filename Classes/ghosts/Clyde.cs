using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using PacTheMan.Models;

namespace pactheman_server {

    class Clyde : Ghost {

        public Clyde(Position startPos, MoveInstruction instruction) : base(instruction) {
            this.Position = startPos;
            this.StartPosition = Position;
            this.MovesToMake = new List<Position>();
            this.lastTarget = StartPosition;
        }

        public override async Task<dynamic> Move(Player targetOne, Player targetTwo) {
            if (Waiting) return Position;

            await Task.Yield();

            Actor target = new ClosestAggression().SelectTarget(this, targetOne, targetTwo);
            Position targetPos = target.Position;
            switch (this.CurrentGhostState) {
                case GhostStates.Chase:
                    targetPos = lastTarget;
                    if (MovesToMake.IsEmpty()) MovesToMake = moveInstruction.GetMoves(this, target);
                    if (Position.IsEqualUpToRange(lastTarget, 5)) {
                        targetPos = lastTarget = MovesToMake.Pop();
                    }
                    Velocity = targetPos.SubOther(Position);
                    Position.AddOther(Velocity.Normalize().Multiply(MovementSpeed).Multiply(delta));
                    if ((await base.Move(targetOne, targetTwo)).Item1) return null;
                    return Position;
                case GhostStates.Scatter:
                    // move to lower left corner
                    targetPos = lastTarget;
                    if (Position.IsEqualUpToRange(lastTarget, 5)) {
                        try {
                            targetPos = lastTarget = MovesToMake.Pop();
                        } catch (ArgumentOutOfRangeException) {
                            CurrentGhostState = GhostStates.Chase;
                            break;
                        }
                    }
                    if (scatterTicker >= SCATTER_SECONDS) {
                        MovesToMake = moveInstruction.GetMoves(this, target);
                        CurrentGhostState = GhostStates.Chase;
                        scatterTicker = 0;
                        break;
                    }
                    Velocity = targetPos.SubOther(Position);
                    Position.AddOther(Velocity.Normalize().Multiply(MovementSpeed).Multiply(delta));
                    scatterTicker += delta;
                    if ((await base.Move(targetOne, targetTwo)).Item1) return null;
                    return Position;
                case GhostStates.Frightened:
                    if ((await base.Move(targetOne, targetTwo)).Item1) return null;
                    return Position;
            }
            if ((await base.Move(targetOne, targetTwo)).Item1) return null;
            return Position;
        }
    }
}