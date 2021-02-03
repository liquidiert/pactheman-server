using Bebop.Attributes;
using Bebop.Runtime;
using PacTheMan.Models;
using System;
using System.Linq;
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
                await session.clients[(Guid)readyMsg.Session.ClientId]
                    .GetStream().WriteAsync(ErrorCodes.UnknownSession);
                return;
            }

            await session.clients
                .First(c => c.Key != (Guid)readyMsg.Session.ClientId)
                    .Value.GetStream().WriteAsync(
                        new NetworkMessage {
                            IncomingOpCode = ReadyMsg.OpCode,
                            IncomingRecord = new ReadyMsg().EncodeAsImmutable()
                        }.Encode()
                    );

        }
    }
}