using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Collisions;
using MonoGame.Extended.Sprites;
using PacTheMan.Models;

namespace pactheman_server {
    public class Player : Actor {

        public Guid Id { get; set; }
        public int Score = 0;
        protected int _lives = 3;
        public string Lives {
            get => "<3".Multiple(_lives);
        }
        public MovingState CurrentMovingState {
            get { return movingState; }
            set {
                if (movingState != value) {
                    movingState = value;
                    switch (movingState) {
                        case MovingState.Up:
                            Sprite.Play("up");
                            break;
                        case MovingState.Down:
                            Sprite.Play("down");
                            break;
                        case MovingState.Left:
                            Sprite.Play("left");
                            break;
                        case MovingState.Right:
                            Sprite.Play("right");
                            break;
                    }
                }
            }
        }
        public Vector2 StatsPosition { get; set; }

        public Player(ContentManager content, string name, string spriteLocation) : base(content, spriteLocation) {
            this.Name = name;
            this.Position = GameEnv.Instance.PlayerStartPoints.Pop(new Random().Next(GameEnv.Instance.PlayerStartPoints.Count)).Position;
            this.StartPosition = Position;
            this.Sprite.Play(this.Position.X < 1120 ? "right" : "left");
        }

        public override void Move(GameTime t) { }

        public override void Draw(SpriteBatch b) {
            b.Draw(Sprite, Position);
        }
        public override void Reset() {
            Velocity = Vector2.Zero;
            Position = StartPosition;
        }
        public override void Clear() {
            _lives = 3;
            Position = GameEnv.Instance.PlayerStartPoints.Pop(new Random().Next(GameEnv.Instance.PlayerStartPoints.Count)).Position; ;
            StartPosition = Position;
            Sprite.Play(this.Position.X < 1120 ? "right" : "left");
        }
        public override void OnCollision(CollisionInfo collisionInfo) {
            Position -= collisionInfo.PenetrationVector;
            base.OnCollision(collisionInfo);
        }

        public void DecreaseLives() {
            _lives--;
        }

    }
}