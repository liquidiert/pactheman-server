using PacTheMan.Models;

namespace pactheman_server {
    public class ClosestAggression : Aggression {
        public Actor SelectTarget(Actor aggressor, Actor possibleTarget1, Actor possibleTarget2) {
            return aggressor.Position.Distance(possibleTarget1.Position) < aggressor.Position.Distance(possibleTarget2.Position) ?
                possibleTarget1 : possibleTarget2;
        }
    }
}