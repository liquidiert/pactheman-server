using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using PacTheMan.Models;

namespace pactheman_server {

    class Pinky : Ghost {

        public Pinky(Position startPos, MoveInstruction instruction) : base(instruction) {
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
                    if (Position.IsEqualUpToRange(lastTarget, 5f)) {
                        try {
                            targetPos = lastTarget = MovesToMake.Pop().Multiply(64).Add(32);
                        } catch (ArgumentOutOfRangeException) {
                            MovesToMake = moveInstruction.GetMoves(this, target, elapsedSeconds: (float)delta);
                            if (MovesToMake.IsEmpty()) { // hussa pacman reached!
                                CurrentGhostState = GhostStates.Scatter;
                                MovesToMake = AStar.Instance.GetPath(DownScaledPosition, new Position {  X= 1, Y = 1 });
                                break;
                            }
                            targetPos = lastTarget = MovesToMake.Pop().Multiply(64).Add(32);
                        }
                    }
                    Velocity = new Position { X = targetPos.X, Y = targetPos.Y }.SubOther(Position).Normalize();
                    Position.AddOther(Velocity.Multiply(MovementSpeed).Multiply(delta));
                    if ((await base.Move(targetOne, targetTwo)).Item1) return null;
                    return Position;
                case GhostStates.Scatter:
                    // move to upper left corner
                    targetPos = lastTarget;
                    if (Position.IsEqualUpToRange(lastTarget, 5f)) {
                        try {
                            targetPos = lastTarget = MovesToMake.Pop().Multiply(64).Add(32);
                        } catch (ArgumentOutOfRangeException) {
                            CurrentGhostState = GhostStates.Chase;
                            break;
                        }
                    }
                    if (scatterTicker >= SCATTER_SECONDS) {
                        MovesToMake = moveInstruction.GetMoves(this, target, elapsedSeconds: (float)delta);
                        CurrentGhostState = GhostStates.Chase;
                        scatterTicker = 0;
                        break;
                    }
                    Velocity = new Position { X = targetPos.X, Y = targetPos.Y }.SubOther(Position).Normalize();
                    Position.AddOther(Velocity.Multiply(MovementSpeed).Multiply(delta));
                    scatterTicker += (float)delta;
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