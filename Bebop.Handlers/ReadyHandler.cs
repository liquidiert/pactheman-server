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
            if (!(session?.playerOneReady.Task.IsCompleted ?? true)) {
                session?.playerOneReady.TrySetResult(true);
            } else if (!(session?.playerTwoReady.Task.IsCompleted ?? true)) {
                session?.playerTwoReady.TrySetResult(true);
            }

            await session.clients
                .Where(c => c.Key != (readyMsg.Session.ClientId ?? Guid.NewGuid())).First()
                    .Value.GetStream().WriteAsync(
                        new NetworkMessage {
                            IncomingOpCode = ReadyMsg.OpCode,
                            IncomingRecord = new ReadyMsg().EncodeAsImmutable()
                        }.Encode()
                    );

        }
    }
}