using Bebop.Runtime;
using System;
using System.IO;
using System.Net;
using System.Linq;
using PacTheMan.Models;
using System.Threading;
using System.Net.Sockets;
using System.Collections.Concurrent;

namespace pactheman_server {
    public class SessionListener : TcpListener {

        private ConcurrentDictionary<Guid, Session> sessions;

        SessionListener(IPAddress _ip, Int32 _port = 8080) : base(_ip, _port) {
            // init concurrent session dict with 30 possible sessions
            sessions = new ConcurrentDictionary<Guid, Session>(Environment.ProcessorCount * 3, 30);
        }

        public void Listen() {
            Start();

            Byte[] buffer = new Byte[256];

            try {
                while (true) {
                    Console.WriteLine("Waiting for a connection... ");

                    // Perform a blocking call to accept requests.
                    var client = AcceptTcpClient();

                    // call handler func in thread that either creates a session or assigns client to an existing session 
                    ThreadPool.QueueUserWorkItem(new WaitCallback(AddSession), client);
                }
            } catch (SocketException ex) {
                Console.WriteLine("SocketException: {0}", ex);
            } finally {
                Stop();
            }

        }

        public void AddSession(object clientObj) {
            var client = (TcpClient)clientObj;
            var stream = client.GetStream();

            Byte[] buffer = new Byte[256];

            // blocking call for first message
            stream.Read(buffer);

            NetworkMessage msg = NetworkMessage.Decode(buffer);

            if (msg.IncomingOpCode != Join.OpCode) {
                stream.WriteAsync(ErrorCodes.UnexpectedMessage);
                return;
            }

            Join joinMsg = Join.Decode(msg.IncomingRecord);

            Guid sessionId;
            if (joinMsg.Session != null) {
                sessionId = (Guid)joinMsg.Session.SessionId;
                var clientTwoId = Guid.NewGuid();
                sessions[sessionId].clients.AddOrUpdate(clientTwoId, (id) => new Tuple<TcpClient, PlayerState>(client, new PlayerState()), (id, tuple) => tuple);
                var clientOne = sessions[sessionId].clients.Where((pair) => pair.Key != clientTwoId).First();
                //clientOne.Value.Item1.GetStream().Write();
            } else {
                sessionId = Guid.NewGuid();
                sessions.TryAdd(sessionId, new Session((GhostAlgorithms)joinMsg.Algorithms));
                sessions[sessionId].clients = new ConcurrentDictionary<Guid, Tuple<TcpClient, PlayerState>>(Environment.ProcessorCount * 2, 2);
                var clientId = Guid.NewGuid();
                sessions[sessionId].clients.TryAdd(clientId, new Tuple<TcpClient, PlayerState>(client, new PlayerState()));
                stream.Write(new NetworkMessage {
                        IncomingOpCode = SessionMsg.OpCode,
                        IncomingRecord = new SessionMsg { SessionId = sessionId, ClientId = clientId }.EncodeAsImmutable()
                    }.Encode()
                );
            }
        }

    }
}