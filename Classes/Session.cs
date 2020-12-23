using System;
using System.Linq;
using System.Net.Sockets;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using PacTheMan.Models;
using Bebop.Runtime;

namespace pactheman_server {

    public class Session {

        public ConcurrentDictionary<Guid, Tuple<TcpClient, PlayerState>> clients;
        private GhostAlgorithms ghostAlgorithmsToUse;
        public TaskCompletionSource<bool> playerOneReady;
        public TaskCompletionSource<bool> playerTwoReady;

        public Session(GhostAlgorithms algorithms) {

            ghostAlgorithmsToUse = algorithms;

        }

        public async Task<bool> Run() {

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

                    var ghostState = await generateGhostMoves();
                    var ghostMove = new GhostMove {
                        State = ghostState
                    };
                    var netMsg = new NetworkMessage {
                        IncomingOpCode = GhostMove.OpCode,
                        IncomingRecord = ghostMove.EncodeAsImmutable()
                    };

                    Task sendGhostsClientOne = clients[firstClientId].Item1.GetStream().WriteAsync(netMsg.Encode()).AsTask();
                    Task sendGhostsClientTwo = clients[secondClientId].Item1.GetStream().WriteAsync(netMsg.Encode()).AsTask();

                    Task.WaitAll(sendGhostsClientOne, sendGhostsClientTwo);
                    
                }
            } catch (SocketException ex) {
                Console.WriteLine(ex);
            }

            return false;

        }

        async Task<GhostState> generateGhostMoves() {
            var b = ghostAlgorithmsToUse.Blinky;
            return new GhostState();
        }

        async Task clientListener(Guid clientId) {

            Byte[] buffer = new Byte[4096];

            while (true) {
                await clients[clientId].Item1.GetStream().ReadAsync(buffer);
                var message = NetworkMessage.Decode(buffer);
                BebopMirror.HandleRecord(message.IncomingRecord.ToArray(), message.IncomingOpCode ?? 0, this);
            }
        }

    }

}