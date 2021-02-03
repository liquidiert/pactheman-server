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

            var clientId = (Guid)playerState.Session.ClientId;
            var client = session.clients[clientId];
            var otherClient = session.clients.First(c => c.Key != clientId).Value;

            if (
                ((Position)session.state.PlayerPositions[clientId]).IsEqualUpToRange((Position)playerState.PlayerPositions[clientId]) ||
                    (playerState.PlayerPositions[clientId].X < 70 || playerState.PlayerPositions[clientId].X > 1145) // player went through portal
                ) {
                try {
                    session.state.PlayerPositions[clientId] = (Position)playerState.PlayerPositions[clientId];
                    session.state.Directions[clientId] = playerState.Direction;

                    var player = GameEnv.Instance.Players.Find(p => p.Id == clientId);
                    player.Position = session.state.PlayerPositions[clientId].ToVec2();

                    if (GameEnv.Instance.RemoveScorePoint(player.Position)) {
                        player.Score += 10;
                        session.state.Scores[clientId] = player.Score;
                    }

                    var msg = new NetworkMessage {
                        IncomingOpCode = PlayerState.OpCode,
                        IncomingRecord = session.state.GeneratePlayerState(clientId, (SessionMsg)playerState.Session).EncodeAsImmutable()
                    }.Encode();
                    await otherClient.GetStream().WriteAsync(msg);
                    //await client.GetStream().WriteAsync(msg);
                } catch (Exception ex) {
                    Console.WriteLine(ex.ToString());
                }
            } else {
                await client.GetStream().WriteAsync(ErrorCodes.InvalidPosition);
            }

        }
    }
}