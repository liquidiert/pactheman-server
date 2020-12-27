using Bebop.Attributes;
using Bebop.Runtime;
using PacTheMan.Models;
using System;
using System.Threading.Tasks;

namespace pactheman_server {

    [RecordHandler]
    public static class PlayerStateHandler {

        [BindRecord(typeof(BebopRecord<PlayerState>))]
        public static async Task HandlePlayerStateUpdate(object sessionObj, PlayerState playerState) {

            Session session = (Session) sessionObj;
            if (playerState.Session.SessionId == null) return;

            var clientId = playerState.Session.ClientId ?? Guid.NewGuid();
            var client = session.clients[clientId];
            var clientStream = client.Item1.GetStream();

            if (((Position) client.Item2.PlayerPositions[clientId]).IsEqualUpToRange((Position) playerState.PlayerPositions[clientId], 2)) {
                client.Item2.PlayerPositions[clientId] = playerState.PlayerPositions[clientId];
                //TODO: check for invalid score -> overall possible - other player score == mine ?
                await clientStream.WriteAsync(
                    new NetworkMessage {
                        IncomingOpCode = PlayerState.OpCode,
                        IncomingRecord = client.Item2.EncodeAsImmutable()
                    }.Encode()
                );
            } else {
                await clientStream.WriteAsync(ErrorCodes.InvalidPosition);
            }
            
        }
    }
}