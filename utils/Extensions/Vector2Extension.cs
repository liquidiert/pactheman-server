using System;
using PacTheMan.Models;
using Microsoft.Xna.Framework;

namespace pactheman_server {

    static class Vector2Extension {

        public static Position ToPosition(this Vector2 vector) {
            return new Position { X = vector.X, Y = vector.Y };
        }

        public static Vector2 RealNormalize(this Vector2 vector) {
            if (vector == Vector2.Zero) return vector;
            vector /= vector.Length();
            vector.Round();
            return vector;
        }

        public static Vector2 SubtractValue(this Vector2 vector, int toSubtract) {
            vector.X -= toSubtract;
            vector.Y -= toSubtract;
            return vector;
        }

        public static Vector2 AddValue(this Vector2 vector, int toAdd) {
            vector.X += toAdd;
            vector.Y += toAdd;
            return vector;
        }

        public static Vector2 DivideValue(this Vector2 vector, int toDivide) {
            vector.X /= toDivide;
            vector.Y /= toDivide;
            return vector;
        }

        public static Vector2 CeilInstance(this Vector2 vector) {
            vector.Ceiling();
            return vector;
        }

        public static Vector2 FloorInstance(this Vector2 vector) {
            vector.Floor();
            return vector;
        }

        public static double Distance(this Vector2 vector, Vector2 toCompare) {
            return Math.Sqrt((vector.X-toCompare.X)*(vector.X-toCompare.X) + (vector.Y-toCompare.Y)*(vector.Y-toCompare.Y));
        }

        public static Vector2 Interpolated(this Vector2 vector, Vector2 other) {
            return new Vector2 {
                X = (vector.X + other.X) / 2,
                Y = (vector.Y + other.Y) / 2
            };
        }

    }

}