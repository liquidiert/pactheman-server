using Bebop.Attributes;
using Bebop.Runtime;
using PacTheMan.Models;
using System;
using System.Threading.Tasks;
using System.Net.Sockets;

namespace pactheman_server {

    [RecordHandler]
    public class PlayerStateHandler {

        [BindRecord(typeof(BebopRecord<PlayerState>))]
        public async Task HandlePlayerStateUpdate(object client, PlayerState playerState) {

            NetworkStream stream = (client as TcpClient).GetStream();

            if (playerState.Session.ClientId == null) { // no clientID given -> singleplayer
                if (playerState.Session.SessionId != null) await stream.WriteAsync(ErrorCodes.NoSessionGiven);
                PlayerState currentState;
                SessionHandler.Instance.SingleplayerSessions.TryGetValue(playerState.Session.SessionId ?? Guid.NewGuid(), out currentState);
                BasePosition currentPosition;
                currentState.PlayerPositions.TryGetValue(playerState.Session.SessionId ?? Guid.NewGuid(), out currentPosition);
                if (!currentPosition.IsEqualUpToRange(playerState.PlayerPositions[playerState.Session.SessionId ?? Guid.NewGuid()])) {
                    await stream.WriteAsync(ErrorCodes.InvalidPosition);
                }
            } else { // clientID given -> multiplayer

            }

            (client as TcpClient).Close();
        }
    }
}