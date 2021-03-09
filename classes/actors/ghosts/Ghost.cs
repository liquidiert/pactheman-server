using System;
using System.Linq;
using System.Timers;
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
            this.MovementSpeed = 450f; // 80
            GameState.Instance.StateChanged += async (object sender, GameStateEvent args) => {
                if (args.CurrentState == GameStates.Game) {
                    await Task.Delay(TimeSpan.FromMilliseconds(new Random().Next(5000)))
                        .ContinueWith(task => Waiting = false);
                }
            };
            _scatterTimer = new Timer(new Random().Next(5000, 20000));
            _scatterTimer.Elapsed += (source, args) => {
                if (Waiting) return;
                Targets = AStar.Instance.GetPath(DownScaledPosition, scatterTarget);
                lastTarget = (Targets.Pop() * 64).AddValue(32);
                CurrentGhostState = GhostStates.Scatter;
            };
            _scatterTimer.AutoReset = true;
            _scatterTimer.Enabled = true;
        }

        private Timer _scatterTimer;

        public bool Waiting = true;
        protected readonly float SCATTER_SECONDS = 3.5f;
        protected float scatterTicker { get; set; }
        protected Vector2 lastTarget { get; set; }
        protected Vector2 scatterTarget { get; set; }
        protected MoveInstruction moveInstruction { get; set; }

        public List<Vector2> Targets = new List<Vector2>();
        protected GhostStates CurrentGhostState = GhostStates.Chase;

        public override void Move(GameTime t) {
            float delta = t.GetElapsedSeconds();
            if (Waiting) return;
            var target = new ClosestAggression().SelectTarget(this);
            Vector2 targetPos;
            switch (this.CurrentGhostState) {
                case GhostStates.Chase:
                    targetPos = lastTarget;
                    if (Targets.IsEmpty()) Targets = GameEnv.Instance.GhostMoveInstructions[Name].GetMoves(this, target);
                    if (Position.EqualsWithTolerence(lastTarget, 5f)) {
                        try {
                            targetPos = lastTarget = (Targets.Pop() * 64).AddValue(32);
                        } catch {
                            Targets = AStar.Instance.GetPath(DownScaledPosition, scatterTarget);
                            lastTarget = (Targets.Pop() * 64).AddValue(32);
                            CurrentGhostState = GhostStates.Scatter;
                            return;
                        }
                    }
                    Velocity = targetPos - Position;
                    Position += Velocity.RealNormalize() * MovementSpeed * delta;
                    break;
                case GhostStates.Scatter:
                    // move to scatter target
                    targetPos = lastTarget;
                    if (Position.EqualsWithTolerence(targetPos, 5f)) {
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
            GameEnv.Instance.Session.State.Lives[args.Collider.Id]--;
            GameEnv.Instance.Players.First(p => p.Id == args.Collider.Id).DecreaseLives();
            if (GameEnv.Instance.Session.State.Lives[args.Collider.Id] == 0) {
                args.Collider.Reward = -10;
                await GameEnv.Instance.Session.SendGameOver(args.Collider.Id);
            } else {
                args.Collider.Reward = -1;
                await GameEnv.Instance.Session.SendCollision();
            }
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