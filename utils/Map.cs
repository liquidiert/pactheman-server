using System;
using System.IO;

namespace pactheman_server {
    public static class Map {
        public static int[,] map = new int[19, 22];

        public static void Init() {
            var mapFileContent = File.ReadAllLines($"{Environment.CurrentDirectory}/map.txt");
            for (var h = 0; h < mapFileContent.Length; h++) {
                for (var w = 0; w < mapFileContent[h].Length; w++) {
                    map[w, h] = int.Parse(mapFileContent[h][w].ToString());
                }
            }
        }
    }
}