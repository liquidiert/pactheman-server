using System;

namespace PacTheMan.Models {

    public static class PositionExtension {

        /// <summary>
        /// Checks wheter a Position "otherPos" is in range of this position "selfPos"
        /// </summary>
        /// <param name="otherPos">The position to check</param>
        /// <param name="range">Range in which the other position still counts as the same; defaults to 32</param>
        /// <returns>A <c>bool<c/> indicating whether position is in range</returns>
        public static bool IsEqualUpToRange(this Position selfPos, Position otherPos, int range = 32) {
            return (selfPos.X - range <= otherPos.X && selfPos.X + range >= otherPos.X) 
                    && (selfPos.Y - range <= otherPos.Y && selfPos.Y + range >= otherPos.Y);
        }

        public static Position Normalize(this Position vector) {
            vector.Divide(Math.Sqrt(Math.Pow(vector.X, 2) + Math.Pow(vector.Y, 2)));
            return vector;
        }

        public static Position AddOther(this Position selfPos, Position other) {
            selfPos.X += other.X;
            selfPos.Y += other.Y;
            return selfPos;
        }

        public static Position SubOther(this Position selfPos, Position other) {
            selfPos.X -= other.X;
            selfPos.Y += other.Y;
            return selfPos;
        }

        public static Position Sub(this Position selfPos, int toMultiply) {
            selfPos.X -= toMultiply;
            selfPos.Y -= toMultiply;
            return selfPos;
        }

        public static Position Sub(this Position selfPos, float toMultiply) {
            selfPos.X -= (int) Math.Ceiling(toMultiply);
            selfPos.Y -= (int) Math.Ceiling(toMultiply);
            return selfPos;
        }

        public static Position Sub(this Position selfPos, double toMultiply) {
            selfPos.X -= (int) Math.Ceiling(toMultiply);
            selfPos.Y -= (int) Math.Ceiling(toMultiply);
            return selfPos;
        }

        public static Position Multiply(this Position selfPos, int toMultiply) {
            selfPos.X *= toMultiply;
            selfPos.Y *= toMultiply;
            return selfPos;
        }

        public static Position Multiply(this Position selfPos, float toMultiply) {
            selfPos.X *= (int) Math.Ceiling(toMultiply);
            selfPos.Y *= (int) Math.Ceiling(toMultiply);
            return selfPos;
        }

        public static Position Multiply(this Position selfPos, double toMultiply) {
            selfPos.X *= (int) Math.Ceiling(toMultiply);
            selfPos.Y *= (int) Math.Ceiling(toMultiply);
            return selfPos;
        }

        public static Position Divide(this Position selfPos, int toMultiply) {
            selfPos.X /= toMultiply;
            selfPos.Y /= toMultiply;
            return selfPos;
        }

        public static Position Divide(this Position selfPos, float toMultiply) {
            selfPos.X /= (int) Math.Ceiling(toMultiply);
            selfPos.Y /= (int) Math.Ceiling(toMultiply);
            return selfPos;
        }

        public static Position Divide(this Position selfPos, double toMultiply) {
            selfPos.X /= (int) Math.Ceiling(toMultiply);
            selfPos.Y /= (int) Math.Ceiling(toMultiply);
            return selfPos;
        }


        public static double Distance(this Position vector, Position toCompare) {
            return Math.Sqrt((vector.X-toCompare.X)*(vector.X-toCompare.X) + (vector.Y-toCompare.Y)*(vector.Y-toCompare.Y));
        }

    }
}