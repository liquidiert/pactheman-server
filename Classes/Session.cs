using System;
using System.Linq;
using System.Net.Sockets;
using System.Net.NetworkInformation;
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
        public ConcurrentDictionary<Guid, TcpClient> clients;
        private GhostAlgorithms ghostAlgorithmsToUse;
        private Dictionary<String, Ghost> ghosts;
        public TaskCompletionSource<bool> playerOneReady;
        public TaskCompletionSource<bool> playerTwoReady;
        private List<Position> PossibleGhostStartPositions;
        private Task _sessionTask;
        private CancellationTokenSource _ctRunSource;
        private CancellationToken _ctRun;
        private Action<string> _endSession;
        private SessionState _sessionState;
        private Thread _clientOneLoop;
        private Thread _clientTwoLoop;
        public SessionState state {
            get => _sessionState;
        }
        private static List<String> ghostNames = new List<string> {
            "blinky",
            "clyde",
            "inky",
            "pinky"
        };

        public Session(string id, GhostAlgorithms algorithms, Action<string> endSession) {

            Id = id;
            _endSession = endSession;

            clients = new ConcurrentDictionary<Guid, TcpClient>(Environment.ProcessorCount * 3, 2);

            PossibleGhostStartPositions = new List<Position>();
            PossibleGhostStartPositions.AddMany(
                new Position { X = 608, Y = 544 },
                new Position { X = 544, Y = 672 },
                new Position { X = 608, Y = 672 },
                new Position { X = 672, Y = 672 }
            );
            ghostAlgorithmsToUse = algorithms;
            ghosts = new Dictionary<String, Ghost>();
            ghosts.Add("blinky", new Blinky(
                PossibleGhostStartPositions.Pop(new Random().Next(PossibleGhostStartPositions.Count)),
                MoveInstruction.FromString(ghostAlgorithmsToUse.Blinky)
            ));
            ghosts.Add("clyde", new Clyde(
                PossibleGhostStartPositions.Pop(new Random().Next(PossibleGhostStartPositions.Count)),
                MoveInstruction.FromString(ghostAlgorithmsToUse.Clyde)
            ));
            ghosts.Add("inky", new Inky(
                PossibleGhostStartPositions.Pop(new Random().Next(PossibleGhostStartPositions.Count)),
                MoveInstruction.FromString(ghostAlgorithmsToUse.Inky)
            ));
            ghosts.Add("pinky", new Pinky(
                PossibleGhostStartPositions.Pop(new Random().Next(PossibleGhostStartPositions.Count)),
                MoveInstruction.FromString(ghostAlgorithmsToUse.Pinky)
            ));

            _sessionState = new SessionState();
            _sessionState.GhostPositions = ghosts.ToDictionary(gP => gP.Key, gP => gP.Value.StartPosition);

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
                foreach (var client in clients) {
                    client.Value.Dispose();
                }
                clients.Clear();
                ghosts.Clear();
                _sessionState.Dispose();
                PossibleGhostStartPositions.Clear();
                _ctRunSource.Dispose();
            }

            _disposed = true;
        }

        public async Task WelcomeClients(Guid joineeId, string joineeName) {

            var clientOneId = clients.Keys.First(c => c != joineeId);

            // send join to host
            await clients[clientOneId].GetStream().WriteAsync(new NetworkMessage {
                IncomingOpCode = PlayerJoinedMsg.OpCode,
                IncomingRecord = new PlayerJoinedMsg {
                    PlayerName = _sessionState.Names[joineeId]
                }.EncodeAsImmutable()
            }.Encode());
            // send join to client two
            await clients[joineeId].GetStream().WriteAsync(new NetworkMessage {
                IncomingOpCode = PlayerJoinedMsg.OpCode,
                IncomingRecord = new PlayerJoinedMsg {
                    PlayerName = _sessionState.Names[clientOneId],
                    Session = new SessionMsg {
                        SessionId = Id,
                        ClientId = joineeId
                    }
                }.EncodeAsImmutable()
            }.Encode());

            // set initial state
            _sessionState.SetPlayerPositions(clientOneId, joineeId);

            _sessionState.Directions = new Dictionary<Guid, MovingStates> {
                {clientOneId, _sessionState.PlayerPositions[clientOneId].X < 1120 ? MovingStates.Right : MovingStates.Left},
                {joineeId, _sessionState.PlayerPositions[joineeId].X < 1120 ? MovingStates.Right : MovingStates.Left}
            };

            foreach (var clientId in new List<Guid> { clientOneId, joineeId }) {
                _sessionState.Lives.Add(clientId, 3);
                _sessionState.Scores.Add(clientId, 0);
            }
        }

        public async Task Run() {
            _sessionTask = Task.Run(() => _run(), _ctRun);
            await _sessionTask;
        }

        private async Task _run() {
            // Were we already canceled?
            _ctRun.ThrowIfCancellationRequested();

            try {
                var clientKeys = clients.Keys;

                var firstClientId = clientKeys.Take(1).First();
                var secondClientId = clientKeys.TakeLast(1).First();

                _clientOneLoop = new Thread(() => clientListener(firstClientId, secondClientId));
                _clientTwoLoop = new Thread(() => clientListener(secondClientId, firstClientId));

                _clientOneLoop.Start();
                _clientTwoLoop.Start();

                playerOneReady = new TaskCompletionSource<bool>();
                playerTwoReady = new TaskCompletionSource<bool>();

                // wait for players to get ready
                Task.WaitAll(playerOneReady.Task, playerTwoReady.Task);

                var initState = _sessionState.GenerateInitState(firstClientId, secondClientId);
                foreach (var client in clients) {
                    var netMessage = new NetworkMessage {
                        IncomingOpCode = InitState.OpCode,
                        IncomingRecord = initState.EncodeAsImmutable()
                    };
                    await client.Value.GetStream().WriteAsync(netMessage.Encode());
                }

                foreach (var ghost in ghosts) {
#pragma warning disable 4014 // -> must run asynchronously for all ghosts
                    Task.Delay(TimeSpan.FromMilliseconds(new Random().Next(5000)))
                        .ContinueWith(task => ghost.Value.Waiting = false);
#pragma warning restore
                }

                // "blocking" ghost stream
                while (true) {

                    if (_ctRun.IsCancellationRequested) {
                        _ctRun.ThrowIfCancellationRequested();
                    }

                    var playerOne = new Player {
                        Position = (Position)_sessionState.PlayerPositions[firstClientId],
                        Lives = (int)_sessionState.Lives[firstClientId]
                    };

                    var playerTwo = new Player {
                        Position = (Position)_sessionState.PlayerPositions[secondClientId],
                        Lives = (int)_sessionState.Lives[secondClientId]
                    };

                    var resetAndGhostStateTuple = await generateGhostMoves(
                        playerOne,
                        playerTwo
                    );

                    NetworkMessage networkMessage;
                    if (!resetAndGhostStateTuple.Item1) {
                        var ghostMove = new GhostMoveMsg {
                            State = resetAndGhostStateTuple.Item2
                        };
                        networkMessage = new NetworkMessage {
                            IncomingOpCode = GhostMoveMsg.OpCode,
                            IncomingRecord = ghostMove.EncodeAsImmutable()
                        };
                    } else {
                        var reset = new ResetMsg {
                            PlayerLives = new Dictionary<Guid, long> {
                                {firstClientId, playerOne.Lives},
                                {secondClientId, playerTwo.Lives}
                            }
                        };
                        networkMessage = new NetworkMessage {
                            IncomingOpCode = ResetMsg.OpCode,
                            IncomingRecord = reset.EncodeAsImmutable()
                        };
                    }

                    Task sendGhostsClientOne = clients[firstClientId].GetStream().WriteAsync(networkMessage.Encode(), _ctRun).AsTask();
                    Task sendGhostsClientTwo = clients[secondClientId].GetStream().WriteAsync(networkMessage.Encode(), _ctRun).AsTask();

                    Task.WaitAll(sendGhostsClientOne, sendGhostsClientTwo);

                    await Task.Delay(167);

                }
            } catch (InvalidOperationException) {
                // swallow -> failed to send ghost move due to connection to client killed
            } catch (OperationCanceledException) {
                // swallow -> canceled thread
            } catch (Exception ex) {
                Console.WriteLine(ex.ToString());
            }
        }

        /// <summary>
        /// Generates new move targets for each ghost.
        /// </summary>
        /// <param name="playerOne">Target one</param>
        /// <param name="playerTwo">Target two</param>
        /// <returns>
        /// <param name="resetAndStateTuple">A tuple of bool (indicating wheter to trigger reset) and ghost state</param>
        /// </returns>
        private async Task<Tuple<bool, GhostState>> generateGhostMoves(Player playerOne, Player playerTwo) {
            var state = new GhostState();
            state.Positions = new Dictionary<string, BasePosition>();
            var targets = new Dictionary<string, Task<dynamic>>();
            foreach (var name in ghostNames) {
                //TODO: add target deletion for sudden change
                targets.Add(name, ghosts[name].Move(playerOne, playerTwo));
            }
            while (targets.Count > 0) {
                var key = "blinky";
                var finishedTask = await Task.WhenAny(targets.Values);

                Task<dynamic> toCompare;
                if (targets.TryGetValue("clyde", out toCompare) && finishedTask == toCompare) {
                    key = "clyde";
                } else if (targets.TryGetValue("inky", out toCompare) && finishedTask == toCompare) {
                    key = "inky";
                } else if (targets.TryGetValue("pinky", out toCompare) && finishedTask == toCompare) {
                    key = "pinky";
                }

                if (finishedTask.Result != null) {
                    state.Positions.Add(key, finishedTask.Result);
                } else { // collision
                    targets.Clear();
                    // safe to return because we already awaited 
                    return new Tuple<bool, GhostState>(true, state);
                }
                targets.Remove(key);
            }
            targets.Clear();
            return new Tuple<bool, GhostState>(false, state);
        }

        private async void clientListener(Guid clientOneId, Guid clientTwoId) {

            // already canceled?
            _ctRun.ThrowIfCancellationRequested();

            Byte[] buffer = new Byte[2048];

            Console.WriteLine($"Started listening for {clientOneId}");

            try {
                TcpClient client;
                while (clients.TryGetValue(clientOneId, out client) && client.GetState() == TcpState.Established) {

                    if (_ctRun.IsCancellationRequested) {
                        _ctRun.ThrowIfCancellationRequested();
                    }

                    var size = await client.GetStream().ReadAsync(buffer, _ctRun);
                    var message = NetworkMessage.Decode(buffer);
                    BebopMirror.HandleRecord(message.IncomingRecord.ToArray(), message.IncomingOpCode ?? 0, this);
                }

                // a client disconnected -> inform other client and dispose session
                TcpClient clientTwo;
                if (clients.TryGetValue(clientTwoId, out clientTwo) && clientTwo.GetState() == TcpState.Established) {
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
                    await clientTwo.GetStream().WriteAsync(netMessage.Encode());
                }

                this._endSession(this.Id);
            } catch (OperationCanceledException) {
                // swallow -> canceled thread
            } catch (Exception ex) {
                Console.WriteLine(ex.ToString());
            }
        }

    }

}