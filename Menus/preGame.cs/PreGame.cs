using Microsoft.Xna.Framework;
using MonoGame.Extended;
using MonoGame.Extended.Gui;
using MonoGame.Extended.Gui.Controls;
using System.Threading;

namespace pactheman_server {
    class PreGameMenu : Screen {

        public string Name = "PreGameMenu";
        private Label sessionId;

        public void UpdateSessionId(string update) {
            this.sessionId.Content = update;
        }
        
        public PreGameMenu() {
            sessionId = new Label();
            this.Content = new StackPanel {
                Height = 500,
                Padding = new Thickness(50, 0),
                BackgroundColor = Color.Black,
                BorderColor = Color.Yellow,
                BorderThickness = 2,
                Orientation = Orientation.Vertical,
                VerticalAlignment = VerticalAlignment.Centre,
                HorizontalAlignment = HorizontalAlignment.Centre,
                Items = {
                    new Label("The session id is:"),
                    sessionId
                }
            };
        }
    }
}