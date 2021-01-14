using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace pactheman_server {
    class HumanPlayer : Player {
        public HumanPlayer(ContentManager content, string name) : base(content, name, "sprites/player/spriteFactory.sf") {
            this.StatsPosition = new Vector2(-350, 50);
            Name = name;
        }
    }
}