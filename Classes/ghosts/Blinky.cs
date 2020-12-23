/* using System;
using System.Collections.Generic;
using PacTheMan.Models;

namespace pactheman_server {

    class Blinky : Ghost {

        public Blinky(string name, Position startPos) {
            this.Position = startPos;
            this.StartPosition = Position;
            this.Name = name;
            this.MovesToMake = new List<Position>();
            this.lastTarget = StartPosition;
        }

        public override void Move() {
            if (Waiting) return;

            Position target;
            switch (this.CurrentGhostState) {
                case GhostStates.Chase:
                    target = lastTarget;
                    if (Position.IsEqualUpToRange(lastTarget, 1)) {
                        try {
                            target = lastTarget = MovesToMake.Pop();
                        } catch (ArgumentOutOfRangeException) {
                            MovesToMake = Environment.Instance.GhostMoveInstructions[Name].GetMoves();
                            if (MovesToMake.IsEmpty()) { // hussa pacman reached!
                                CurrentGhostState = GhostStates.Scatter;
                                MovesToMake = AStar.Instance.GetPath(Position, new Position { X = 17, Y = 1 });
                                break;
                            }
                            target = lastTarget = MovesToMake.Pop();
                        }
                    }
                    Velocity = target.SubOther(Position);
                    Position.AddOther(Velocity.Normalize().Multiply(MovementSpeed).Multiply(delta));
                    break;
                case GhostStates.Scatter:
                    // move to upper right corner
                    target = lastTarget;
                    if (Position.IsEqualUpToRange(lastTarget, 1)) {
                        try {
                            target = lastTarget = MovesToMake.Pop();
                        } catch (ArgumentOutOfRangeException) {
                            CurrentGhostState = GhostStates.Chase;
                            break;
                        }
                    }
                    if (scatterTicker >= SCATTER_SECONDS) {
                        MovesToMake = Environment.Instance.GhostMoveInstructions[Name].GetMoves();
                        CurrentGhostState = GhostStates.Chase;
                        scatterTicker = 0;
                        break;
                    }
                    Velocity = target.SubOther(Position);
                    Position.AddOther(Velocity.Normalize().Multiply(MovementSpeed).Multiply(delta));
                    scatterTicker += delta;
                    break;
                case GhostStates.Frightened:
                    break;
            }
        }
    }
} */