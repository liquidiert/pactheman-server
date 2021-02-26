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
                ((Position)session.State.PlayerPositions[clientId]).IsEqualUpToRange((Position)playerState.PlayerPositions[clientId]) ||
                    (playerState.PlayerPositions[clientId].X < 70 || playerState.PlayerPositions[clientId].X > 1145) // player went through portal
                ) {
                try {
                    session.State.PlayerPositions[clientId] = (Position)playerState.PlayerPositions[clientId];
                    session.State.Directions[clientId] = playerState.Direction;

                    var player = GameEnv.Instance.Players.Find(p => p.Id == clientId) as Player;
                    player.Position = session.State.PlayerPositions[clientId].ToVec2();
                    player.CurrentMovingState = playerState.Direction;

                    if (GameEnv.Instance.RemoveScorePoint(player.Position)) {
                        player.Score += 10;
                        session.State.Scores[clientId] = player.Score;
                    }

                    var msg = new NetworkMessage {
                        IncomingOpCode = PlayerState.OpCode,
                        IncomingRecord = session.State.GeneratePlayerState(clientId, (SessionMsg)playerState.Session).EncodeAsImmutable()
                    }.Encode();
                    await otherClient.GetStream().WriteAsync(msg, 0, msg.Length);
                    await Task.Delay(5); // ensure buffer is safe
                    await client.GetStream().WriteAsync(msg, 0, msg.Length);
                } catch (Exception ex) {
                    Console.WriteLine(ex.ToString());
                }
            } else {
                GameEnv.Instance.Session.State.Strikes[clientId]++;
                if (GameEnv.Instance.Session.State.Strikes[clientId] >= 3) {
                    var netMessage = new NetworkMessage {
                        IncomingOpCode = GameOverMsg.OpCode,
                        IncomingRecord = new GameOverMsg {
                            Reason = GameOverReason.ExceededStrikes,
                            PlayerId = clientId
                        }.EncodeAsImmutable()
                    };
                    await client.GetStream().WriteAsync(netMessage.Encode());
                    await otherClient.GetStream().WriteAsync(netMessage.Encode());
                    return;
                }
                await client.GetStream().WriteAsync(ErrorCodes.InvalidPosition);
            }

        }
    }
}