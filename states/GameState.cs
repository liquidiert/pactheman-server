using System;

namespace pactheman_server {

    public enum GameStates {
        MainMenu,
        PreGameMenu,
        Game,
        GameReset,
        GamePaused
    }

    class GameState {
        public GameStates CurrentGameState { get; set; }
        public float RESET_COUNTER = 4;

        private static readonly Lazy<GameState> lazy = new Lazy<GameState>(() => new GameState());
        public static GameState Instance { get => lazy.Value; }
        private GameState() {}

    }
}