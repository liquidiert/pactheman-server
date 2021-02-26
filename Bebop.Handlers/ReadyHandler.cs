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
    public static class ReadyHandler {

        [BindRecord(typeof(BebopRecord<ReadyMsg>))]
        public static async Task HandleReady(object sessionObj, ReadyMsg readyMsg) {
            Session session = (Session)sessionObj;
            if (readyMsg.Session.SessionId == null) return;

            // kinda naive but we can't influence Ready directly unfortunately
            if (readyMsg.Session.ClientId == session.FirstClientId) {
                session?.playerOneReady.TrySetResult(true);
            } else if (readyMsg.Session.ClientId == session.SecondClientId) {
                session?.playerTwoReady.TrySetResult(true);
            } else {
                await session.Sockets[(Guid)readyMsg.Session.ClientId]
                    .SendAsync(ErrorCodes.UnknownSession, WebSocketMessageType.Binary, true, CancellationToken.None);
                return;
            }

            await session.Sockets
                .First(c => c.Key != (Guid)readyMsg.Session.ClientId)
                    .Value.SendAsync(
                        new NetworkMessage {
                            IncomingOpCode = ReadyMsg.OpCode,
                            IncomingRecord = new ReadyMsg().EncodeAsImmutable()
                        }.Encode(), WebSocketMessageType.Binary, true, CancellationToken.None
                    );

        }
    }
}