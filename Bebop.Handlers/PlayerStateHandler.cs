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

            var player = GameEnv.Instance.Players.Find(p => p.Id == clientId) as Player;
            if (
                ((Position)session.State.PlayerPositions[clientId]).IsEqualUpToRange((Position)playerState.PlayerPositions[clientId]) ||
                    (playerState.PlayerPositions[clientId].X < 70 || playerState.PlayerPositions[clientId].X > 1145) // player went through portal
                ) {
                try {
                    session.State.PlayerPositions[clientId] = (Position)playerState.PlayerPositions[clientId];
                    session.State.Directions[clientId] = playerState.Direction;

                    player.Reward = 0;
                    player.Position = session.State.PlayerPositions[clientId].ToVec2();
                    player.CurrentMovingState = playerState.Direction;

                    if (GameEnv.Instance.RemoveScorePoint(player.Position)) {
                        player.Score += 10;
                        player.Reward = 1;
                        session.State.Scores[clientId] = player.Score;
                    }

                    var msg = new NetworkMessage {
                        IncomingOpCode = PlayerState.OpCode,
                        IncomingRecord = session.State.GeneratePlayerState(clientId, (SessionMsg)playerState.Session).EncodeAsImmutable()
                    }.Encode();
                    //await otherClient.SendAsync(msg, WebSocketMessageType.Binary, true, CancellationToken.None);
                    await client.SendAsync(msg, WebSocketMessageType.Binary, true, CancellationToken.None);
                } catch (Exception ex) {
                    Console.WriteLine(ex.ToString());
                }
            } else {
                GameEnv.Instance.Session.State.Strikes[clientId]++;

                var netMessage = new NetworkMessage {
                        IncomingOpCode = StrikeMsg.OpCode,
                        IncomingRecord = new StrikeMsg {
                            Reason = "invalid position",
                            PlayerId = clientId,
                            StrikeCount = GameEnv.Instance.Session.State.Strikes[clientId]
                        }.EncodeAsImmutable()
                    }.Encode();

                if (GameEnv.Instance.Session.State.Strikes[clientId] >= 3) {
                    await GameEnv.Instance.Session.SendGameOver(clientId);
                    return;
                } else {
                    player.Reward = -5;
                    await client.SendAsync(netMessage, WebSocketMessageType.Binary, true, CancellationToken.None);
                }

                (session.State.PlayerPositions[clientId] as Position).Print();
                (playerState.PlayerPositions[clientId] as Position).Print();
            }

        }
    }
}