using Microsoft.Xna.Framework;

namespace pactheman_server {
    public class ClosestAggression : Aggression {
        public Actor SelectTarget(Actor aggressor) {
            if (aggressor.Position.Distance(GameEnv.Instance.Actors["player"].Position) < aggressor.Position.Distance(GameEnv.Instance.Actors["opponent"].Position)) {
                return GameEnv.Instance.Actors["player"];
            } else {
                return GameEnv.Instance.Actors["opponent"];
            }
        }
    }
}