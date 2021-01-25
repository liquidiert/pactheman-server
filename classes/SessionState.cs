using System.Collections.Generic;
using PacTheMan.Models;
using System;
using System.Linq;

namespace pactheman_server {
    public class SessionState : IDisposable {
        private bool _disposed;
        public Dictionary<Guid, string> Names { get; set; }
        public Dictionary<Guid, MovingStates> Directions { get; set; }
        public Dictionary<Guid, long> Scores { get; set; }
        public Dictionary<Guid, long> Lives { get; set; }
        public Dictionary<Guid, Position> PlayerPositions { get; set; }

        public SessionState() {
            Names = new Dictionary<Guid, string>();
            Directions = new Dictionary<Guid, MovingStates>();
            Scores = new Dictionary<Guid, long>();
            Lives = new Dictionary<Guid, long>();
            PlayerPositions = new Dictionary<Guid, Position>();
        }


        public void Dispose() => Dispose(true);
        protected void Dispose(bool disposing) {
            if (_disposed) return;

            if (disposing) {
                Names.Clear();
                Directions.Clear();
                Scores.Clear();
                Lives.Clear();
                PlayerPositions.Clear();
            }

            _disposed = true;
        }

        public void SetPlayerPositions(Guid clientOne, Guid clientTwo) {
            PlayerPositions = new Dictionary<Guid, Position> {
                {clientOne, GameEnv.Instance.Actors["player"].Position.ToPosition()},
                {clientTwo, GameEnv.Instance.Actors["opponent"].Position.ToPosition()}
            };
        }

        public InitState GenerateInitState(Guid clientOne, Guid clientTwo) {
            return new InitState {
                GhostInitPositions = GameEnv.Instance.Ghosts.ToDictionary(item => item.Name, item => (BasePosition)item.Position.ToPosition()),
                PlayerInitPositions = PlayerPositions.ToDictionary(item => item.Key, item => (BasePosition)item.Value),
                ScorePointInitPositions = GameEnv.Instance.ScorePointPositions.Select(p => (BasePosition)p.Position.ToPosition()).ToArray(),
                PlayerInitLives = Lives,
                PlayerInitScores = Scores
            };
        }

        public PlayerState GeneratePlayerState(Guid client, SessionMsg session) {
            return new PlayerState {
                Session = session,
                Direction = Directions[client],
                Scores = Scores,
                Lives = Lives,
                PlayerPositions = new Dictionary<Guid, Position>(PlayerPositions)
                    .ToDictionary(item => item.Key, item => (BasePosition)item.Value)
            };
        }
    }
}