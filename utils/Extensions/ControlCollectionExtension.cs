using MonoGame.Extended.Gui.Controls;

namespace pactheman_server {
    public static class ElementCollectionExtension {
        public static void AddAll(this ControlCollection collection, Control[] toAdd) {
            foreach (var control in toAdd) {
                collection.Add(control);
            }
        }
    }
}