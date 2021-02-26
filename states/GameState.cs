using System;

namespace pactheman_server {

    public enum GameStates {
        MainMenu,
        PreGameMenu,
        Game,
        GameReset,
        GamePaused
    }

    public class GameStateEvent : EventArgs {
        public GameStates CurrentState { get; set; }

        public GameStateEvent(GameStates currentState) => CurrentState = currentState;
    }

    class GameState {
        private GameStates _currGameState { get; set; }
        public GameStates CurrentGameState { 
            get => _currGameState;
            set {
                _currGameState = value;
                StateChanged?.Invoke(this, new GameStateEvent(_currGameState));
            }
        }
        public float RESET_COUNTER = 4;

        private static readonly Lazy<GameState> lazy = new Lazy<GameState>(() => new GameState());
        public static GameState Instance { get => lazy.Value; }
        private GameState() {}

        public event EventHandler<GameStateEvent> StateChanged;

    }
}