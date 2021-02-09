using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using PacTheMan.Models;

namespace pactheman_server {

    public enum GhostStates {
        Scatter,
        Chase,
        Frightened
    }

    public class Ghost : Actor {

        public Ghost(ContentManager content, string spriteSheeLocation) : base(content, spriteSheeLocation) {
            this.MovementSpeed = 80f;
            UIState.Instance.StateChanged += async (object sender, UIStateEvent args) => {
                if (args.CurrentState == UIStates.Game) {
                    await Task.Delay(TimeSpan.FromMilliseconds(new Random().Next(5000)))
                        .ContinueWith(task => Waiting = false);
                }
            };
        }

        public bool Waiting = true;
        protected readonly float SCATTER_SECONDS = 3.5f;
        protected float scatterTicker { get; set; }
        protected Vector2 lastTarget { get; set; }
        public Vector2 LastTarget {
            get => lastTarget;
            set => lastTarget = value;
        }
        protected Vector2 scatterTarget { get; set; }
        protected MoveInstruction moveInstruction { get; set; }

        public List<Vector2> Targets = new List<Vector2>();
        protected GhostStates CurrentGhostState = GhostStates.Chase;

        public override void Move(GameTime t) {
            float delta = t.GetElapsedSeconds();
            if (Waiting) return;
            var target = new ClosestAggression().SelectTarget(this);
            Vector2 targetPos = target.Position;
            switch (this.CurrentGhostState) {
                case GhostStates.Chase:
                    targetPos = lastTarget;
                    if (Targets.IsEmpty()) Targets = GameEnv.Instance.GhostMoveInstructions[Name].GetMoves(this, target);
                    if (Position.EqualsWithTolerence(lastTarget, 5f)) {
                        try {
                            targetPos = lastTarget = (Targets.Pop() * 64).AddValue(32);
                        } catch {
                            Targets = AStar.Instance.GetPath(DownScaledPosition, scatterTarget);
                            lastTarget = Targets?.Pop() ?? Position;
                            CurrentGhostState = GhostStates.Scatter;
                            return;
                        }
                    }
                    Velocity = targetPos - Position;
                    Position += Velocity.RealNormalize() * MovementSpeed * delta;
                    break;
                case GhostStates.Scatter:
                    // move to lower left corner
                    targetPos = lastTarget;
                    if (Position.EqualsWithTolerence(lastTarget, 5f)) {
                        try {
                            targetPos = lastTarget = (Targets.Pop() * 64).AddValue(32);
                        } catch (ArgumentOutOfRangeException) {
                            CurrentGhostState = GhostStates.Chase;
                            scatterTicker = 0;
                            break;
                        }
                    }
                    if (scatterTicker >= SCATTER_SECONDS) {
                        Targets = GameEnv.Instance.GhostMoveInstructions[Name].GetMoves(this, target);
                        CurrentGhostState = GhostStates.Chase;
                        scatterTicker = 0;
                        break;
                    }
                    Velocity = targetPos - Position;
                    Position += Velocity.RealNormalize() * MovementSpeed * delta;
                    scatterTicker += delta;
                    break;
                case GhostStates.Frightened:
                    break;
            }

        }
        public virtual async void OnActorCollision(object sender, CollisionPairEvent args) {
            GameEnv.Instance.Session.state.Lives[args.Collider.Id]--;
            GameEnv.Instance.Players.First(p => p.Id == args.Collider.Id).DecreaseLives();
            await GameEnv.Instance.Session.SendCollision();
            GameEnv.Instance.Reset();
        }
        public override void Draw(SpriteBatch b) { }
        public override void Reset() {
            Velocity = Vector2.Zero;
            Position = StartPosition;
            Targets.Clear();
            lastTarget = Position;
        }
        public override void Clear() {
            Waiting = true;
            Velocity = Vector2.Zero;
            StartPosition = GameEnv.Instance.GhostStartPoints
                .Pop(new Random().Next(GameEnv.Instance.GhostStartPoints.Count)).Position.AddValue(32);
            Position = StartPosition;
            lastTarget = Position;
            Targets.Clear();
        }

    }

}