using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace pactheman_server {
    class Opponent : Player {

        public Opponent(ContentManager content, string name) : base(content, name, "sprites/opponent/spriteFactory.sf") {
            this.StatsPosition = new Vector2(1300, 50);
        }

    }
}