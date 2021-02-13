using System;
using System.Net;
using System.Linq;
using PacTheMan.Models;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Sockets;
using RandomStringCreator;

namespace pactheman_server {
    public class SessionListener : TcpListener {

        private StringCreator randomStringCreator;

        public SessionListener(IPAddress _ip, Int32 _port = 5387) : base(_ip, _port) {
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

        public async void AddSession(object clientObj) {
            var client = (TcpClient)clientObj;
            var stream = client.GetStream();

            Byte[] buffer = new Byte[256];

            // blocking call for first message
            await stream.ReadAsync(buffer);

            NetworkMessage msg = NetworkMessage.Decode(buffer);

            if (msg.IncomingOpCode != JoinMsg.OpCode) {
                if (msg.IncomingOpCode == ReconnectMsg.OpCode) {
                    ReconnectMsg reconnect = ReconnectMsg.Decode(msg.IncomingRecord);
                    if (GameEnv.Instance.Session.Id != reconnect.Session.SessionId) {
#pragma warning disable 4014 // -> we don't care about errors just continue
                        stream.WriteAsync(ErrorCodes.UnknownSession);
#pragma warning restore
                    }
                    GameEnv.Instance.Session.clients.AddOrUpdate((Guid)reconnect.Session.ClientId, (id) => client, (id, c) => client);
                    return;
                } else {
#pragma warning disable 4014 // -> we don't care about errors just continue
                    stream.WriteAsync(ErrorCodes.UnexpectedMessage);
#pragma warning restore
                    return;
                }
            }

            client.NoDelay = true;
            JoinMsg joinMsg = JoinMsg.Decode(msg.IncomingRecord);

            string sessionId;
            if (joinMsg.Session != null) {
                if (GameEnv.Instance.Session.Id != joinMsg.Session.SessionId) {
#pragma warning disable 4014 // -> we don't care about errors just continue
                    stream.WriteAsync(ErrorCodes.UnknownSession);
#pragma warning restore
                    return;
                }
                if (GameEnv.Instance.Session.clients.Count > 1) {
                    // already two players in lobby; refuse other connection tries
#pragma warning disable 4014 // -> we don't care about errors just continue
                    stream.WriteAsync(ErrorCodes.ToManyPlayers);
#pragma warning restore
                    return;
                }

                // add second client to session
                var clientTwoId = Guid.NewGuid();
                GameEnv.Instance.Session.clients.AddOrUpdate(clientTwoId, (id) => client, (id, c) => c);
                var clientOne = GameEnv.Instance.Session.clients.First((pair) => pair.Key != clientTwoId);
                GameEnv.Instance.Session.State.Names.TryAdd(clientTwoId, joinMsg.PlayerName);
                GameEnv.Instance.Session.State.Strikes.TryAdd(clientTwoId, 0);
                GameEnv.Instance.Actors["opponent"].Name = joinMsg.PlayerName;
                await stream.WriteAsync(new NetworkMessage {
                    IncomingOpCode = SessionMsg.OpCode,
                    IncomingRecord = new SessionMsg { SessionId = joinMsg.Session.SessionId, ClientId = clientTwoId }.EncodeAsImmutable()
                }.Encode());

                await GameEnv.Instance.Session.WelcomeClients(clientTwoId, joinMsg.PlayerName);

                // start session
#pragma warning disable 4014 // -> session must run in separate thread
                Task.Run(() => GameEnv.Instance.Session.Start());
#pragma warning restore
                Stop(); // close session listener
            } else {
                sessionId = randomStringCreator.Get(6);
                GameEnv.Instance.Session = new Session(sessionId);
                var clientId = Guid.NewGuid();
                GameEnv.Instance.Session.clients.TryAdd(clientId, client);
                await stream.WriteAsync(new NetworkMessage {
                    IncomingOpCode = SessionMsg.OpCode,
                    IncomingRecord = new SessionMsg { SessionId = sessionId, ClientId = clientId }.EncodeAsImmutable()
                }.Encode());
                GameEnv.Instance.Session.State.Names.TryAdd(clientId, joinMsg.PlayerName);
                GameEnv.Instance.Session.State.Strikes.TryAdd(clientId, 0);
                GameEnv.Instance.Actors["player"].Name = joinMsg.PlayerName;
                (UIState.Instance.CurrentScreen as PreGameMenu).UpdateSessionId(sessionId);
            }
        }

    }
}