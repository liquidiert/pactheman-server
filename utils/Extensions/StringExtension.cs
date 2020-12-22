using System;

namespace pactheman_server {

    static class StringExtension {
        public static string Multiple(this string s, int factor) {
            var res = "";
            for (var i = 0; i < factor; i++) res += s;
            return res;
        }
    }

}