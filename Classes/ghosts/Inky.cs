/* using System;
using System.Collections.Generic;
using PacTheMan.Models;

namespace pactheman_server {

    class Inky : Ghost {

        public Inky(string name, Position startPos) {
            this.Position = startPos;
            this.StartPosition = Position;
            this.Name = name;
            this.MovesToMake = new List<Position>();
            this.lastTarget = StartPosition;
        }
        public override void Move() {
            if (Waiting) return;
            float delta = gameTime.GetElapsedSeconds();

            Position target;
            switch (this.CurrentGhostState) {
                case GhostStates.Chase:
                    target = lastTarget;
                    if (MovesToMake.IsEmpty()) MovesToMake = Environment.Instance.GhostMoveInstructions[Name].GetMoves();
                    if (Position.IsEqualUpToRange(lastTarget, 1)) {
                        target = lastTarget = MovesToMake.Pop();
                    }
                    Velocity = target.SubOther(Position);
                    
                    break;
                case GhostStates.Scatter:
                    // move to lower right corner
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