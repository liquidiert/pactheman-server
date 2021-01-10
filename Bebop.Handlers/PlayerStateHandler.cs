using Bebop.Attributes;
using Bebop.Runtime;
using PacTheMan.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace pactheman_server {

    [RecordHandler]
    public static class PlayerStateHandler {

        [BindRecord(typeof(BebopRecord<PlayerState>))]
        public static async Task HandlePlayerStateUpdate(object sessionObj, PlayerState playerState) {

            Session session = (Session)sessionObj;
            if (playerState.Session.SessionId == null) return;
            Console.WriteLine("got state");

            var clientId = playerState.Session.ClientId ?? Guid.NewGuid();
            var client = session.clients[clientId];
            var otherClient = session.clients.Where(c => c.Key != clientId).First().Value;
            var clientStream = client.GetStream();

            if (((Position)session.state.PlayerPositions[clientId]).IsEqualUpToRange((Position)playerState.PlayerPositions[clientId], 2)) {
                session.state.PlayerPositions[clientId] = (Position)playerState.PlayerPositions[clientId];
                //TODO: check for invalid score -> overall possible - other player score == mine ?
                var msg = new NetworkMessage {
                    IncomingOpCode = PlayerState.OpCode,
                    IncomingRecord = session.state.GeneratePlayerState(clientId).EncodeAsImmutable()
                }.Encode();
                await clientStream.WriteAsync(msg);
                await otherClient.GetStream().WriteAsync(msg);
            } else {
                await clientStream.WriteAsync(ErrorCodes.InvalidPosition);
            }

        }
    }
}