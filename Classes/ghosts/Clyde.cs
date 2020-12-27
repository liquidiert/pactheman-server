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

        public override async Task<Position> Move(Actor targetOne, Actor targetTwo) {
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
                    break;
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
                    break;
                case GhostStates.Frightened:
                    return Position;
                default:
                    return Position;
            }
            return Position;
        }
    }
}