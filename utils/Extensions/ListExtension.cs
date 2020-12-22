using System;
using System.Linq;
using System.Collections.Generic;

namespace pactheman_server {
    static class ListExtension {

        public static bool IsEmpty<T>(this List<T> list) {
            return list.Count == 0;
        }
        public static void Print<T>(this List<T> list) {
            foreach (var el in list.Select((entry, ind) => new { entry = entry, ind = ind }).ToList()) {
                Console.WriteLine($"{el.ind} {el.entry}");
            }
            Console.WriteLine("-------------------");
        }
        public static T Pop<T>(this List<T> list, int? index = null) {
            index ??= list.Count - 1;
            T r = list[(int) index];
            list.RemoveAt((int) index);
            return r;
        }
        public static List<T> AddMany<T>(this List<T> list, params T[] toAdd) {
            foreach(var obj in toAdd) {
                list.Add(obj);
            }
            return list;
        }
        public static bool RemoveWhere<T>(this List<T> list, Predicate<T> removeCondition) {
            var entry = list.Find(removeCondition);
            return list.Remove(entry);
        }
        ///
        /// <returns>Index of the minimal member</returns>
        ///
        public static int MinIndex <T>(this List<T> list) where T: IComparable<T> {
            dynamic min = Int32.MaxValue;
            int res = 0;
            foreach (var el in list.Select((entry, ind) => new { entry = entry, ind = ind})) {
                if (el.entry < min) {
                    min = el.entry;
                    res = el.ind;
                }
            }
            return res;
        }

    }
}