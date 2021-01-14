using MonoGame.Extended.Gui;
using System;

namespace pactheman_server {
    public enum UIStates {
        Menu,
        MainMenu,
        Settings,
        PreGame,
        HostOrJoin,
        PreLobby,
        Lobby,
        InGame,
        Game
    }
    public class UIStateEvent : EventArgs {
        public UIStates CurrentState { get; set; }

        public UIStateEvent(UIStates currentState) => CurrentState = currentState;
    }
    public class UIState {

        public Screen PreviousScreen { get; set; }
        public Screen CurrentScreen {
            get {
                return GuiSystem.ActiveScreen;
            }
            set {
                PreviousScreen = CurrentScreen;
                GuiSystem.ActiveScreen = value;
            }
        }
        public Screen MainMenu { get; set; }
        public GuiSystem GuiSystem { get; set; }
        private UIStates _currentUIState { get; set; }
        public UIStates CurrentUIState { 
            get => _currentUIState; 
            set {
                _currentUIState = value;
                StateChanged?.Invoke(this, new UIStateEvent(_currentUIState));
            }
        }
        public event EventHandler<UIStateEvent> StateChanged; // IDEA: do more with that?

        private static readonly Lazy<UIState> lazy = new Lazy<UIState>(() => new UIState());
        public static UIState Instance { get => lazy.Value; }
        private UIState() {}
    }
}