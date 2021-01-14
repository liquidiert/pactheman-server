using Microsoft.Xna.Framework;

namespace pactheman_server {

    class Node {

        public Node Parent;
        public Vector2 Position;

        public double g = 0;
        public double h = 0;
        public double f = 0;

        public Node(Node parent = null, Vector2? position = null) => (Parent, Position) = (parent, (Vector2) position);

        public override bool Equals(object toCompare) {
            if (toCompare == null) return false;
            if (!(toCompare is Node)) return false;
            return this.Position == ((Node) toCompare).Position;
        }

        public override int GetHashCode() {
            return Position.GetHashCode() ^ g.GetHashCode() ^ h.GetHashCode() ^ f.GetHashCode();
        }

        # nullable enable
        public static bool operator ==(Node? a, Node? b) {
            return (a ??= new Node(position: new Vector2(-1, -1))).Position
                == (b ??= new Node(position: new Vector2(-1, -1))).Position;
        }

        # nullable enable
        public static bool operator !=(Node? a, Node? b) {
            return (a ??= new Node(position: new Vector2(-1, -1))).Position
                != (b ??= new Node(position: new Vector2(-1, -1))).Position;
        }
    }

}