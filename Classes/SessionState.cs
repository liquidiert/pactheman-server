using System.Collections.Generic;
using PacTheMan.Models;
using System;
using System.Linq;

namespace pactheman_server {
    public class SessionState : IDisposable {
        private bool _disposed;
        public Dictionary<Guid, string> Names { get; set; }
        public Dictionary<Guid, long> ReconciliationIds { get; set; }
        public Dictionary<Guid, long> Scores { get; set; }
        public Dictionary<Guid, long> Lives { get; set; }
        public Dictionary<string, Position> GhostPositions { get; set; }
        public Dictionary<Guid, Position> PlayerPositions { get; set; }

        public SessionState() {
            Names = new Dictionary<Guid, string>();
            ReconciliationIds = new Dictionary<Guid, long>();
            Scores = new Dictionary<Guid, long>();
            Lives = new Dictionary<Guid, long>();
            GhostPositions = new Dictionary<string, Position>();
            PlayerPositions = new Dictionary<Guid, Position>();
        }

        private List<Position> _possiblePlayerStartPositions = new List<Position> {
            new Position {X = 1118, Y = 90},
            new Position {X = 90, Y = 90},
            new Position {X = 1118, Y = 1310},
            new Position {X = 90, Y = 1310}
        };


        public void Dispose() => Dispose(true);
        protected void Dispose(bool disposing) {
            if (_disposed) return;

            if (disposing) {
                Names.Clear();
                ReconciliationIds.Clear();
                Scores.Clear();
                Lives.Clear();
                GhostPositions.Clear();
                PlayerPositions.Clear();
            }

            _disposed = true;
        }

        public void SetPlayerPositions(Guid clientOne, Guid clientTwo) {
            PlayerPositions = new Dictionary<Guid, Position> {
                {clientOne, _possiblePlayerStartPositions.Pop(new Random().Next(_possiblePlayerStartPositions.Count))},
                {clientTwo, _possiblePlayerStartPositions.Pop(new Random().Next(_possiblePlayerStartPositions.Count))}
            };
        }

        public InitState GenerateInitState(Guid clientOne, Guid clientTwo) {
            return new InitState {
                StartReconciliationId = ReconciliationIds,
                GhostInitDelays = new Dictionary<string, long> {
                    {"blinky", new Random().Next(5)},
                    {"clyde", new Random().Next(5)},
                    {"inky", new Random().Next(5)},
                    {"pinky", new Random().Next(5)}
                },
                GhostInitPositions = GhostPositions.ToDictionary(item => item.Key, item => (BasePosition)item.Value),
                PlayerInitPositions = PlayerPositions.ToDictionary(item => item.Key, item => (BasePosition)item.Value),
                PlayerInitLives = Lives,
                PlayerInitScores = Scores
            };
        }

        public PlayerState GeneratePlayerState(Guid client, SessionMsg session) {
            return new PlayerState {
                Session = session,
                Name = Names[client],
                ReconciliationId = ReconciliationIds[client],
                Score = Scores,
                Lives = Lives,
                GhostPositions = GhostPositions.ToDictionary(item => item.Key, item => (BasePosition)item.Value),
                PlayerPositions = PlayerPositions.ToDictionary(item => item.Key, item => (BasePosition)item.Value)
            };
        }
    }
}