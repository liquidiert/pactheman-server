using PacTheMan.Models;
using System;

namespace pactheman_server {

    public class Actor {

        public float MovementSpeed = 350f;

        public Position StartPosition;
        public Position Position;
        public Position Velocity = new Position { X = 0, Y = 0 };

        protected MovingStates movingState { get; set; }

        public Position DownScaledPosition {
            get { return new Position { X = (float)Math.Floor(Position.X / 64), Y = (float)Math.Floor(Position.Y / 64) }; }
        }
        protected Position UpdatePosition(float x = 0, int xFactor = 1, float y = 0, int yFactor = 1) {
            return new Position { X = (int)(this.Position.X + x) * xFactor, Y = (int)(this.Position.Y + y) * yFactor };
        }
        /// <summary>
        /// Actors future position using its' current speed and direction
        /// </summary>
        /// <returns>Actors future position as Position</returns>
        public Position FuturePosition(float elapsedSeconds) {
            return DownScaledPosition.AddOther(Velocity.Multiply(MovementSpeed).Multiply(elapsedSeconds));
        }

        public dynamic Describe() {
            return new { pos = Position, speed = MovementSpeed };
        }
        public new string ToString() {
            return $"posX: {(ushort)Position.X} posY: {(ushort)Position.Y}\nvelocity: {Velocity}\n";
        }
    }
}