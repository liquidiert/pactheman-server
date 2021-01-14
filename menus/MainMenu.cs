using Microsoft.Xna.Framework;
using MonoGame.Extended;
using MonoGame.Extended.Gui;
using MonoGame.Extended.Gui.Controls;
using System;

namespace pactheman_server {
    public class MainMenu : Screen {

        public string Name = "MainMenu";
        
        public MainMenu(Action exitGameAction) {

            // game start btn
            var gameStartBtn = new Button {
                        BackgroundColor = Color.Transparent,
                        Content = "Game Start",
                        Margin = new Thickness(0, 50)
                    };
            
            gameStartBtn.Clicked += (sender, args) => {
                UIState.Instance.CurrentUIState = UIStates.PreGame;
                UIState.Instance.CurrentScreen = new PreGameMenu();
            };

            // exit btn
            var exitBtn = new Button {
                        BackgroundColor = Color.Transparent,
                        Content = "Exit Server",
                        Margin = new Thickness(0, 50)
                    };
            exitBtn.Clicked += (sender, args) => exitGameAction();

            this.Content = new StackPanel {
                Orientation = Orientation.Vertical,
                VerticalAlignment = VerticalAlignment.Centre,
                HorizontalAlignment = HorizontalAlignment.Centre,
                Items = {
                    new Label("Pac-The-Man Server") {
                        BackgroundColor = Color.Transparent,
                        TextColor = new Color(255, 211, 0),
                        Margin = new Thickness(0, 0, 0, 50)
                    },
                    gameStartBtn,
                    exitBtn,
                }
            };
        }
    }
}