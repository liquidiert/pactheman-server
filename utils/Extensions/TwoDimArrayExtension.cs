using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace pactheman_server {

    static class TwoDimArrayExtension {

        public static void Print<T>(this T[,] arr) {
            for (var h = 0; h < arr.GetLength(1); h++) {
                for (var w = 0; w < arr.GetLength(0); w++) {
                    Console.Write($"{arr[w, h]} ");
                }
                Console.WriteLine("");
            }
        }

        /// <summary>
        /// Get a section of the 2-dim array arround a center point
        /// </summary>
        ///
        /// <param name="center">Point around which to build region</param>
        /// <param name="regionSize">Must be odd number</param>
        /// <typeparam name="T"></typeparam>
        /// <returns>
        /// A array of tuples consisting of the original position of the element and the element itself;
        /// if size is 1 returns a tuple with the center and the element of the array at the center
        /// </returns>
        /// <exception cref="System.ArgumentException">Thrown when regionSize is not odd</exception>
        public static dynamic GetRegion<T>(this T[,] arr, Vector2 center, int regionSize=1) {
            if (regionSize % 2 == 0) throw new ArgumentException();
            if (regionSize == 1) return new Tuple<Vector2, T>(center, arr[(int) center.X, (int) center.Y]);
            var res = new Tuple<Vector2, T>[regionSize, regionSize];
            var sizeFactor = Math.Floor((float) regionSize / 2);
            var xIndex = 0;
            var yIndex = 0;
            for (int h = (int) (center.Y - sizeFactor); h <= center.Y + sizeFactor; h++) {
                for (int w = (int) (center.X - sizeFactor); w <= center.X + sizeFactor; w++) {
                    res[xIndex, yIndex] = new Tuple<Vector2, T>(new Vector2(w, h), arr[w, h]);
                    xIndex++;
                }
                xIndex = 0;
                if (h >= center.Y - sizeFactor && h <= center.Y + sizeFactor) yIndex ++;
            }
            return res;
        }

        public static List<T> Where<T>(this T[,] arr, Predicate<T> condition) {
            var res = new List<T>();
            for (var h = 0; h < arr.GetLength(1); h++) {
                for (var w = 0; w < arr.GetLength(0); w++) {
                   if (condition(arr[w, h])) res.Add(arr[w, h]);
                }
            }
            return res;
        }

    }
}
