using Bebop.Attributes;
using Bebop.Runtime;
using PacTheMan.Models;
using System;
using System.Linq;
using System.Threading;
using System.Net.WebSockets;
using System.Threading.Tasks;

namespace pactheman_server {

    [RecordHandler]
    public static class PlayerStateHandler {

        [BindRecord(typeof(BebopRecord<PlayerState>))]
        public static async Task HandlePlayerStateUpdate(object sessionObj, PlayerState playerState) {

            Session session = (Session)sessionObj;

            var clientId = (Guid)playerState.Session.ClientId;
            var client = session.Sockets[clientId];
            var otherClient = session.Sockets.First(c => c.Key != clientId).Value;

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
                    await otherClient.SendAsync(msg, WebSocketMessageType.Binary, true, CancellationToken.None);
                    await client.SendAsync(msg, WebSocketMessageType.Binary, true, CancellationToken.None);
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
                    }.Encode();
                    await client.SendAsync(netMessage, WebSocketMessageType.Binary, true, CancellationToken.None);
                    await otherClient.SendAsync(netMessage, WebSocketMessageType.Binary, true, CancellationToken.None);
                    return;
                }
                await client.SendAsync(ErrorCodes.InvalidPosition, WebSocketMessageType.Binary, true, CancellationToken.None);
            }

        }
    }
}