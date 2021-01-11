using PacTheMan.Models;

namespace pactheman_server {

    class Node {

        public Node Parent;
        public Position Position;

        public double g = 0;
        public double h = 0;
        public double f = 0;

        # nullable enable
        public Node(Node? parent = null, Position? position = null) => (Parent, Position) = (parent, position);

        # nullable disable

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
            var minusPos = new Position {X = -1, Y = -1};
            return (a?.Position ?? minusPos)
                == (b?.Position ?? minusPos);
        }

        public static bool operator !=(Node? a, Node? b) {
            var minusPos = new Position {X = -1, Y = -1};
            return (a?.Position ?? minusPos)
                != (b?.Position ?? minusPos);
        }
    }

}