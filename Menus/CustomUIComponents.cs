using MonoGame.Extended;
using MonoGame.Extended.Gui;
using MonoGame.Extended.Gui.Controls;
using Microsoft.Xna.Framework;

namespace pactheman_server {
    public class CustomUIComponents {

        public static Control borderedSection(string sectionTitle, Control[] children, Thickness? margin = null) {
            margin = margin ?? new Thickness(0, 20);
            var panel = new StackPanel {
                Orientation = Orientation.Vertical,
                VerticalAlignment = VerticalAlignment.Centre,
                BorderColor = Color.DarkGray,
                BorderThickness = 2,
                Padding = 10,
                Margin = (Thickness) margin,
                Items = {
                    new Label(sectionTitle) { TextColor = Color.Yellow, Margin = new Thickness(0, 0, 0, 50) }
                }
            };
            panel.Items.AddAll(children);
            return panel;
        }
        public static Control labeledTextBox(string label, Control child, int labelPaddingBottom = 8) {
            return new StackPanel {
                Orientation = Orientation.Horizontal,
                VerticalAlignment = VerticalAlignment.Centre,
                Margin = new Thickness(0, 10),
                Items = {
                        new Label(label) { Padding = new Thickness(0, 0, 20, labelPaddingBottom)},
                        child
                    }
            };
        }

        public static Control labeledListBox(string label, Control child, int labelPaddingBottom = 8) {
            return new StackPanel {
                Orientation = Orientation.Vertical,
                HorizontalAlignment = HorizontalAlignment.Left,
                Padding = new Thickness(0, 20),
                Items = {
                    new Label(label) { Padding = new Thickness(0, 0, 0, labelPaddingBottom)},
                    child
                }
            };
        }
    }
}