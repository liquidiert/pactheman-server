using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using PacTheMan.Models;

namespace pactheman_server {

    class Blinky : Ghost {

        public Blinky(Position startPos, MoveInstruction instruction) : base(instruction) {
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
                    if (Position.IsEqualUpToRange(lastTarget, 1)) {
                        try {
                            targetPos = lastTarget = MovesToMake.Pop();
                        } catch (ArgumentOutOfRangeException) {
                            MovesToMake = moveInstruction.GetMoves(this, target);
                            if (MovesToMake.IsEmpty()) { // hussa pacman reached!
                                CurrentGhostState = GhostStates.Scatter;
                                MovesToMake = AStar.Instance.GetPath(Position, new Position { X = 17, Y = 1 });
                                break;
                            }
                            targetPos = lastTarget = MovesToMake.Pop();
                        }
                    }
                    Velocity = targetPos.SubOther(Position);
                    Position.AddOther(Velocity.Normalize().Multiply(MovementSpeed).Multiply(delta));
                    if ((await base.Move(targetOne, targetTwo)).Item1) return null;
                    return Position;
                case GhostStates.Scatter:
                    // move to upper right corner
                    if (Position.IsEqualUpToRange(lastTarget, 1)) {
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