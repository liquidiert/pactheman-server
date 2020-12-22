using System;

namespace pactheman_server {

    static class MathExtension {
        public static double RoundIf(double ToRound, Func<double, bool> condition) {
            if (condition(ToRound)) return Math.Round(ToRound);
            return ToRound;
        }
    }

}