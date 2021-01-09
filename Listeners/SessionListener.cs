using System;
using System.Net;
using System.Linq;
using PacTheMan.Models;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using System.Collections.Concurrent;
using RandomStringCreator;

namespace pactheman_server {
    public class SessionListener : TcpListener {

        private ConcurrentDictionary<string, Session> sessions;
        private StringCreator randomStringCreator;

        public SessionListener(IPAddress _ip, Int32 _port = 5387) : base(_ip, _port) {
            // init concurrent session dict with 30 possible sessions
            sessions = new ConcurrentDictionary<string, Session>(Environment.ProcessorCount * 3, 30);
            Task.Run(() => SessionWatchdog());
            randomStringCreator = new StringCreator();
        }

        public async Task Listen() {
            Start();

            Byte[] buffer = new Byte[256];

            try {
                while (true) {
                    Console.WriteLine("Waiting for a connection... ");

                    // Perform a semi blocking call to accept requests.
                    var client = await AcceptTcpClientAsync();

                    // call handler func in thread that either creates a session or assigns client to an existing session 
                    ThreadPool.QueueUserWorkItem(new WaitCallback(AddSession), client);
                }
            } catch (SocketException ex) {
                Console.WriteLine("SocketException: {0}", ex);
            } finally {
                Stop();
            }

        }

        private Task SessionWatchdog() {
            while (true) {
                Task.Yield();
                Thread.Sleep(5000);
                foreach (var deadSession in sessions) {
                    if (deadSession.Value.clients.Any(client => client.Value.GetState() != TcpState.Established)) {
                        RemoveSession(deadSession.Value.Id);
                    }
                }
            }
        }

        public async void AddSession(object clientObj) {
            var client = (TcpClient)clientObj;
            var stream = client.GetStream();

            Byte[] buffer = new Byte[256];

            // blocking call for first message
            await stream.ReadAsync(buffer);

            NetworkMessage msg = NetworkMessage.Decode(buffer);

            if (msg.IncomingOpCode != JoinMsg.OpCode) {
#pragma warning disable 4014 // -> we don't care about errors just continue
                stream.WriteAsync(ErrorCodes.UnexpectedMessage);
#pragma warning restore
                return;
            }

            JoinMsg joinMsg = JoinMsg.Decode(msg.IncomingRecord);

            string sessionId;
            if (joinMsg.Session != null) {
                Session session;

                if (!sessions.TryGetValue(joinMsg.Session.SessionId, out session)) {
#pragma warning disable 4014 // -> we don't care about errors just continue
                    stream.WriteAsync(ErrorCodes.UnknownSession);
#pragma warning restore
                    return;
                }
                if (session.clients.Count > 1) {
                    // already two players in lobby; refuse other connection tries
#pragma warning disable 4014 // -> we don't care about errors just continue
                    stream.WriteAsync(ErrorCodes.ToManyPlayers);
#pragma warning restore
                    return;
                }

                // add second client to session
                var clientTwoId = Guid.NewGuid();
                session.clients.AddOrUpdate(clientTwoId, (id) => client, (id, c) => c);
                var clientOne = session.clients.Where((pair) => pair.Key != clientTwoId).First();
                session.state.Names.TryAdd(clientTwoId, joinMsg.PlayerName);

                await session.WelcomeClients(clientTwoId, joinMsg.PlayerName);

                // start session
#pragma warning disable 4014 // -> session must run in separate thread
                Task.Run(() => session.Run());
#pragma warning restore
            } else {
                sessionId = randomStringCreator.Get(6);
                sessions.TryAdd(sessionId, new Session(sessionId, (GhostAlgorithms)joinMsg.Algorithms, RemoveSession));
                var clientId = Guid.NewGuid();
                sessions[sessionId].clients.TryAdd(clientId, client);
                await stream.WriteAsync(new NetworkMessage {
                    IncomingOpCode = SessionMsg.OpCode,
                    IncomingRecord = new SessionMsg { SessionId = sessionId, ClientId = clientId }.EncodeAsImmutable()
                }.Encode());
                sessions[sessionId].state.Names.TryAdd(clientId, joinMsg.PlayerName);
            }
        }

        public void RemoveSession(string id) {
            Console.WriteLine("killing session: " + id);
            Session session;
            sessions.TryRemove(id, out session);
            session.Dispose();
        }

    }
}