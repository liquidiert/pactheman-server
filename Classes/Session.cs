using System;
using System.Linq;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using PacTheMan.Models;
using Bebop.Runtime;

namespace pactheman_server {

    public class Session : IDisposable {

        private bool _disposed = false;
        public Guid Id { get; set; }
        public ConcurrentDictionary<Guid, Tuple<TcpClient, PlayerState>> clients;
        private GhostAlgorithms ghostAlgorithmsToUse;
        private Dictionary<String, Ghost> ghosts;
        public TaskCompletionSource<bool> playerOneReady;
        public TaskCompletionSource<bool> playerTwoReady;
        private List<Position> PossibleGhostStartPositions;
        private Task _sessionTask;
        private CancellationTokenSource _ctRunSource;
        private CancellationToken _ctRun;
        private Action<Guid> _endSession;
        private static List<String> ghostNames = new List<string> {
            "blinky",
            "clyde",
            "inky",
            "pinky"
        };

        public Session(Guid id, GhostAlgorithms algorithms, Action<Guid> endSession) {

            Id = id;
            _endSession = endSession;

            clients = new ConcurrentDictionary<Guid, Tuple<TcpClient, PlayerState>>(Environment.ProcessorCount * 3, 2);

            PossibleGhostStartPositions = new List<Position>();
            PossibleGhostStartPositions.AddMany(
                new Position { X = 9, Y = 8 },
                new Position { X = 8, Y = 10 },
                new Position { X = 9, Y = 10 },
                new Position { X = 10, Y = 10 }
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

            _ctRunSource = new CancellationTokenSource();
            _ctRun = _ctRunSource.Token;

        }

        public void Dispose() => Dispose(true);

        protected virtual void Dispose(bool disposing) {
            if (_disposed) {
                return;
            }

            if (disposing) {
                foreach (var client in clients) {
                    client.Value.Item1.Close();
                }
                clients.Clear();
                ghosts.Clear();
                PossibleGhostStartPositions.Clear();
                if (_ctRun.CanBeCanceled) _ctRunSource.Cancel();
                _ctRunSource.Dispose();
            }

            _disposed = true;
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

                Task firstClientLoop = clientListener(firstClientId);
                Task secondClientLoop = clientListener(secondClientId);

                playerOneReady = new TaskCompletionSource<bool>();
                playerTwoReady = new TaskCompletionSource<bool>();

                // wait for players to get ready
                Task.WaitAll(playerOneReady.Task, playerTwoReady.Task);

                // "blocking" ghost stream
                while (true) {

                    if (_ctRun.IsCancellationRequested) {
                        _ctRun.ThrowIfCancellationRequested();
                    }

                    var playerOne = new Player {
                        Position = (Position)clients[firstClientId].Item2.PlayerPositions[firstClientId],
                        Lives = (int)clients[firstClientId].Item2.Lives[firstClientId]
                    };

                    var playerTwo = new Player {
                        Position = (Position)clients[secondClientId].Item2.PlayerPositions[secondClientId],
                        Lives = (int)clients[secondClientId].Item2.Lives[secondClientId]
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

                    Task sendGhostsClientOne = clients[firstClientId].Item1.GetStream().WriteAsync(networkMessage.Encode()).AsTask();
                    Task sendGhostsClientTwo = clients[secondClientId].Item1.GetStream().WriteAsync(networkMessage.Encode()).AsTask();

                    Task.WaitAll(sendGhostsClientOne, sendGhostsClientTwo);

                }
            } catch (SocketException ex) {
                Console.WriteLine(ex);
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
            var targets = new Dictionary<string, Task<dynamic>>();
            foreach (var name in ghostNames) {
                //TODO: add target deletion for sudden change
                targets.Add(name, ghosts[name].Move(playerOne, playerTwo));
            }
            while (targets.Count > 0) {
                var key = "blinky";
                var finishedTask = await Task.WhenAny(targets.Values);

                if (finishedTask == targets["clyde"]) {
                    key = "clyde";
                } else if (finishedTask == targets["inky"]) {
                    key = "inky";
                } else if (finishedTask == targets["pinky"]) {
                    key = "pinky";
                }
                if (finishedTask.Result != null) {
                    state.Targets.Add(key, finishedTask.Result);
                } else { // collision
                    foreach (var name in ghostNames) {
                        state.ClearTargets.Add(name, true);
                    }
                    targets.Clear();
                    // safe to return because we already awaited 
                    return new Tuple<bool, GhostState>(true, state);
                }

                targets.Remove(key);
            }
            return new Tuple<bool, GhostState>(false, state);
        }

        private async Task clientListener(Guid clientId) {

            Byte[] buffer = new Byte[4096];

            while (clients[clientId].Item1.Connected) {
                await clients[clientId].Item1.GetStream().ReadAsync(buffer);
                var message = NetworkMessage.Decode(buffer);
                BebopMirror.HandleRecord(message.IncomingRecord.ToArray(), message.IncomingOpCode ?? 0, this);
            }

            // a client disconnected -> inform other client and dispose session
            var clientTwo = clients.Where(c => c.Key != clientId).First().Value;
            var exitMsg = new ExitMsg {
                Session = new SessionMsg {
                    SessionId = this.Id,
                    ClientId = clientId
                }
            };
            var netMessage = new NetworkMessage {
                IncomingOpCode = ExitMsg.OpCode,
                IncomingRecord = exitMsg.EncodeAsImmutable()
            };
            await clientTwo.Item1.GetStream().WriteAsync(netMessage.Encode());

            this._endSession(this.Id);
        }

    }

}