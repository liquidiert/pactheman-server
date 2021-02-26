using System;
using System.Linq;
using System.Net.WebSockets;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using PacTheMan.Models;
using Bebop.Runtime;

namespace pactheman_server {

    public class Session : IDisposable {

        private bool _disposed = false;
        public string Id { get; set; }
        public ConcurrentDictionary<Guid, WebSocket> Sockets;
        public TaskCompletionSource<bool> playerOneReady;
        public TaskCompletionSource<bool> playerTwoReady;
        private Task _sessionTask;
        private CancellationTokenSource _ctRunSource;
        private CancellationToken _ctRun;
        private SessionState _sessionState;
        private int _currentLevel = 0;
        private int _currentGame = 0;
        private Thread _clientOneLoop;
        private Thread _clientTwoLoop;
        private Guid _firstClientId;
        private Guid _secondClientId;
        public Guid FirstClientId {
            get => _firstClientId;
        }
        public Guid SecondClientId {
            get => _secondClientId;
        }
        public SessionState State {
            get => _sessionState;
        }

        public Session(string id) {

            Id = id;

            Sockets = new ConcurrentDictionary<Guid, WebSocket>(Environment.ProcessorCount * 3, 2);

            GameEnv.Instance.InitMoveInstructions();

            _sessionState = new SessionState();

            _ctRunSource = new CancellationTokenSource();
            _ctRun = _ctRunSource.Token;

        }

        public void Dispose() => Dispose(true);

        protected void Dispose(bool disposing) {
            if (_disposed) {
                return;
            }

            if (disposing) {
                _ctRunSource.Cancel();
                foreach (var client in Sockets) {
                    client.Value.Dispose();
                }
                Sockets.Clear();
                _sessionState.Dispose();
                _ctRunSource.Dispose();
            }

            _disposed = true;
        }

        public async Task WelcomeClients(Guid joineeId, string joineeName) {

            _firstClientId = Sockets.Keys.First(c => c != joineeId);
            _secondClientId = joineeId;

            // send join to host
            await Sockets[_firstClientId].SendAsync(new NetworkMessage {
                IncomingOpCode = PlayerJoinedMsg.OpCode,
                IncomingRecord = new PlayerJoinedMsg {
                    PlayerName = _sessionState.Names[joineeId]
                }.EncodeAsImmutable()
            }.Encode(), WebSocketMessageType.Binary, true, _ctRun);
            // send join to client two
            await Sockets[joineeId].SendAsync(new NetworkMessage {
                IncomingOpCode = PlayerJoinedMsg.OpCode,
                IncomingRecord = new PlayerJoinedMsg {
                    PlayerName = _sessionState.Names[_firstClientId],
                    Session = new SessionMsg {
                        SessionId = Id,
                        ClientId = joineeId
                    }
                }.EncodeAsImmutable()
            }.Encode(), WebSocketMessageType.Binary, true, _ctRun);

            // set initial state
            _sessionState.SetPlayerPositions(_firstClientId, joineeId);

            _sessionState.Directions = new Dictionary<Guid, MovingState> {
                {_firstClientId, _sessionState.PlayerPositions[_firstClientId].X < 1120 ? MovingState.Right : MovingState.Left},
                {joineeId, _sessionState.PlayerPositions[joineeId].X < 1120 ? MovingState.Right : MovingState.Left}
            };

            foreach (var clientId in new List<Guid> { _firstClientId, joineeId }) {
                _sessionState.Lives.Add(clientId, 3);
                _sessionState.Scores.Add(clientId, 0);
            }
        }

        public async Task Start() {
            _sessionTask = Task.Run(() => _start(), _ctRun);
            await _sessionTask;
        }

        private async Task _start() {
            // Were we already canceled?
            _ctRun.ThrowIfCancellationRequested();

            try {
                _clientOneLoop = new Thread(() => clientListener(_firstClientId, _secondClientId));
                _clientTwoLoop = new Thread(() => clientListener(_secondClientId, _firstClientId));

                _clientOneLoop.Start();
                _clientTwoLoop.Start();

                playerOneReady = new TaskCompletionSource<bool>();
                playerTwoReady = new TaskCompletionSource<bool>();

                (GameEnv.Instance.Actors["player"] as Player).Id = _firstClientId;
                (GameEnv.Instance.Actors["opponent"] as Player).Id = _secondClientId;

                // remove init scorepoints
                GameEnv.Instance.RemoveScorePoint(GameEnv.Instance.Actors["player"].Position);
                GameEnv.Instance.RemoveScorePoint(GameEnv.Instance.Actors["opponent"].Position);

                // wait for players to get ready
                Task.WaitAll(playerOneReady.Task, playerTwoReady.Task);

                var initState = _sessionState.GenerateInitState(_firstClientId, _secondClientId);
                foreach (var client in Sockets) {
                    var netMessage = new NetworkMessage {
                        IncomingOpCode = InitState.OpCode,
                        IncomingRecord = initState.EncodeAsImmutable()
                    };
                    await client.Value.SendAsync(netMessage.Encode(), WebSocketMessageType.Binary, true, _ctRun);
                }

                UIState.Instance.CurrentUIState = UIStates.Game;
                UIState.Instance.GuiSystem.ActiveScreen.Hide();
                GameState.Instance.CurrentGameState = GameStates.Game;

            } catch (InvalidOperationException) {
                // swallow -> failed to send ghost move due to connection to client killed
            } catch (OperationCanceledException) {
                // swallow -> canceled thread
            } catch (Exception ex) {
                Console.WriteLine(ex.ToString());
            }
        }

        public async Task SendGhostPositions(GhostMoveMsg move) {

            var netMessage = new NetworkMessage {
                IncomingOpCode = GhostMoveMsg.OpCode,
                IncomingRecord = move.EncodeAsImmutable()
            };

            foreach (var client in Sockets.Values) {
                await client.SendAsync(netMessage.Encode(), WebSocketMessageType.Binary, true, _ctRun);
            }

        }

        public async Task SendNewLevelMsg() {

            _currentLevel++;

            byte[] netMessage;

            if (_currentLevel >= _sessionState.LevelCount) {
                netMessage = new NetworkMessage {
                    IncomingOpCode = GameOverMsg.OpCode,
                    IncomingRecord = new GameOverMsg {
                        Reason = GameOverReason.ExceededLevelCount
                    }.EncodeAsImmutable()
                }.Encode();
                // end session
                Dispose();
            } else {
                _sessionState.PlayerPositions[FirstClientId] = GameEnv.Instance.Actors["player"].StartPosition.ToPosition();
                _sessionState.PlayerPositions[SecondClientId] = GameEnv.Instance.Actors["opponent"].StartPosition.ToPosition();

                netMessage = new NetworkMessage {
                    IncomingOpCode = NewLevelMsg.OpCode,
                    IncomingRecord = new NewLevelMsg {
                        LevelCount = _currentLevel
                    }.EncodeAsImmutable()
                }.Encode();
            }

            foreach (var client in Sockets.Values) {
                await client.SendAsync(netMessage, WebSocketMessageType.Binary, true, _ctRun);
            }
        }

        public async Task SendCollision() {

            foreach (var player in GameEnv.Instance.Players) {
                _sessionState.PlayerPositions[player.Id] = player.StartPosition.ToPosition();
            }

            var resetMsg = new ResetMsg {
                GhostResetPoints = GameEnv.Instance.Ghosts
                    .ToDictionary(gP => gP.Name, gP => new Position { X = gP.StartPosition.X, Y = gP.StartPosition.Y } as BasePosition),
                PlayerResetPoints = GameEnv.Instance.Players
                    .ToDictionary(pP => pP.Id, pP => new Position { X = pP.StartPosition.X, Y = pP.StartPosition.Y } as BasePosition),
                PlayerLives = _sessionState.Lives
            };

            var netMessage = new NetworkMessage {
                IncomingOpCode = ResetMsg.OpCode,
                IncomingRecord = resetMsg.EncodeAsImmutable()
            }.Encode();

            foreach (var client in Sockets.Values) {
                await client.SendAsync(netMessage, WebSocketMessageType.Binary, true, _ctRun);
            }

        }

        public async Task SendGameOver(Guid looserId) {

            Console.WriteLine($"played game {_currentGame}");
            _currentGame++;

            if (_currentGame >= _sessionState.GameCount) {
                var netMessage = new NetworkMessage {
                    IncomingOpCode = GameOverMsg.OpCode,
                    IncomingRecord = new GameOverMsg {
                        Reason = GameOverReason.ExceededGameCount
                    }.EncodeAsImmutable()
                }.Encode();

                foreach (var client in Sockets.Values) {
                    await client.SendAsync(netMessage, WebSocketMessageType.Binary, true, _ctRun);
                }

                Dispose();

                GameEnv.Instance.Clear();
            } else {
                GameEnv.Instance.NewGame();

                // can be reused cause we just want to reset characters

                var resetMsg = new ResetMsg {
                    GhostResetPoints = GameEnv.Instance.Ghosts
                        .ToDictionary(gP => gP.Name, gP => new Position { X = gP.StartPosition.X, Y = gP.StartPosition.Y } as BasePosition),
                    PlayerResetPoints = GameEnv.Instance.Players
                        .ToDictionary(pP => pP.Id, pP => new Position { X = pP.StartPosition.X, Y = pP.StartPosition.Y } as BasePosition),
                    PlayerLives = _sessionState.Lives
                };

                var netMessage = new NetworkMessage {
                    IncomingOpCode = NewGameMsg.OpCode,
                    IncomingRecord = new NewGameMsg {
                        ResetMsg = resetMsg
                    }.EncodeAsImmutable()
                }.Encode();

                foreach (var client in Sockets.Values) {
                    await client.SendAsync(netMessage, WebSocketMessageType.Binary, true, _ctRun);
                }

                // reset state
                _sessionState.Lives[FirstClientId] = 3;
                _sessionState.Lives[SecondClientId] = 3;
                _sessionState.Scores[FirstClientId] = 0;
                _sessionState.Scores[SecondClientId] = 0;
                _currentLevel = 0;
            }
        }

        private async void clientListener(Guid clientOneId, Guid clientTwoId) {

            // already canceled?
            _ctRun.ThrowIfCancellationRequested();

            Byte[] buffer = new Byte[2048];

            Console.WriteLine($"Started listening for {clientOneId}");

            try {
                WebSocket socket;

                while (Sockets.TryGetValue(clientOneId, out socket)) {

                    if (_ctRun.IsCancellationRequested) {
                        _ctRun.ThrowIfCancellationRequested();
                    }

                    WebSocketReceiveResult res = await socket.ReceiveAsync(buffer, _ctRun);

                    NetworkMessage message;

                    switch (res.MessageType) {
                        case WebSocketMessageType.Close:
                            continue;
                        case WebSocketMessageType.Text:
                            Console.WriteLine(buffer);
                            break;
                        case WebSocketMessageType.Binary:
                            message = NetworkMessage.Decode(buffer);

                            try {
                                BebopMirror.HandleRecord(message.IncomingRecord.ToArray(), message.IncomingOpCode ?? 0, this);
                            } catch (Exception ex) {
                                Console.WriteLine($"{message.IncomingOpCode}: {ex.ToString()}");
                            }
                            break;
                    }

                }

                // a client disconnected -> inform other client and dispose session
                WebSocket socketTwo;
                if (Sockets.TryGetValue(clientTwoId, out socketTwo)) {
                    var exitMsg = new ExitMsg {
                        Session = new SessionMsg {
                            SessionId = this.Id,
                            ClientId = clientOneId
                        }
                    };
                    var netMessage = new NetworkMessage {
                        IncomingOpCode = ExitMsg.OpCode,
                        IncomingRecord = exitMsg.EncodeAsImmutable()
                    };
                    await socketTwo.SendAsync(netMessage.Encode(), WebSocketMessageType.Binary, true, _ctRun);
                }

                Dispose(); // dispose session
            } catch (OperationCanceledException) {
                // swallow -> canceled thread
            } catch (Exception ex) {
                Console.WriteLine(ex.ToString());
            }
        }

    }

}