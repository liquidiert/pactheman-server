using System;
using MonoGame.Extended;

namespace pactheman_server {
    public class CollisionPairEvent : EventArgs {
        public Player Collider { get; set; }

        public CollisionPairEvent(Player collider) => Collider = collider;
    }
    class CollisionPair {

        private Player collidable1;
        private Actor collidable2;

        public bool Enabled { get; set; }
        public int UpdateOrder { get; set; }
        public event EventHandler<CollisionPairEvent> Collision;

        public CollisionPair(Player coll1, Actor coll2) => (collidable1, collidable2) = (coll1, coll2);

        public void Update() {
            if (collidable1.Position.EqualsWithTolerence(collidable2.Position, 32f)) {
                // activate event with dummy args -> collidables already know they are meant
                Collision?.Invoke(this, new CollisionPairEvent(collidable1));
            }
        }

    }
}