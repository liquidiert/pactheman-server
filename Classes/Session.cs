using System;
using System.Linq;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using PacTheMan.Models;
using Bebop.Runtime;

namespace pactheman_server {

    public class Session {

        private ConcurrentDictionary<Guid, Tuple<TcpClient, PlayerState>> clients;
        private static GhostAlgorithms ghostAlgorithmsToUse;

        Session(TcpClient initClient, GhostAlgorithms algorithms) {
            int numProcs = Environment.ProcessorCount;
            int concurrencyLevel = numProcs * 3;
            clients = new ConcurrentDictionary<Guid, Tuple<TcpClient, PlayerState>>(concurrencyLevel, 2);

            ghostAlgorithmsToUse = algorithms;

            // no need to check cause new dict
            clients.TryAdd(Guid.NewGuid(), new Tuple<TcpClient, PlayerState>(initClient, new PlayerState()));

        }

        async Task<bool> Run(TcpClient secondClient) {

            if (!clients.TryAdd(Guid.NewGuid(), new Tuple<TcpClient, PlayerState>(secondClient, new PlayerState()))) {
                // error out and notify client to try again
            }

            try {
                var clientKeys = clients.Keys;

                var firstClientId = clientKeys.Take(1).First();
                var secondClientId = clientKeys.TakeLast(1).First();

                Task firstClientLoop = clientListener(firstClientId);
                Task secondClientLoop = clientListener(secondClientId);

                // "blocking" ghost stream
                while (true) {

                    var ghostState = await generateGhostMoves();
                    var ghostMove = new GhostMove();
                    ghostMove.State = ghostState;
                    var netMsg = new NetworkMessage();
                    netMsg.IncomingOpCode = GhostMove.OpCode;
                    netMsg.IncomingRecord = ghostMove.EncodeAsImmutable();

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

        async Task<bool> clientListener(Guid clientId) {

            Byte[] buffer = new Byte[4096];

            while (true) {
                await clients[clientId].Item1.GetStream().ReadAsync(buffer);
                var message = NetworkMessage.Decode(buffer);
                BebopMirror.HandleRecord(message.IncomingRecord.ToArray(), message.IncomingOpCode ?? 0, clients);
            }
        }

    }

}