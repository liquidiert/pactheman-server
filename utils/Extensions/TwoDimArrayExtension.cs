using System;
using System.Collections.Generic;
using PacTheMan.Models;

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
        /// if size is 1 returns a tuple with the center and the elemnt of the array at the center
        /// </returns>
        public static dynamic GetRegion<T>(this T[,] arr, Position center, int regionSize=1) {
            if (regionSize == 1) return new Tuple<Position, T>(center, arr[(int) center.X, (int) center.Y]);
            var res = new Tuple<Position, T>[regionSize, regionSize];
            var sizeFactor = Math.Floor((float) regionSize / 2);
            var xIndex = 0;
            var yIndex = 0;
            for (int h = (int) (center.Y - sizeFactor); h <= center.Y + sizeFactor; h++) {
                for (int w = (int) (center.X - sizeFactor); w <= center.X + sizeFactor; w++) {
                    res[xIndex, yIndex] = new Tuple<Position, T>(new Position {X = w, Y = h}, arr[w, h]);
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
