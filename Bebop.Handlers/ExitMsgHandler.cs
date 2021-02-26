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
    public static class ExitMsgHandler {

        [BindRecord(typeof(BebopRecord<ExitMsg>))]
        public static async Task HandleExitMsg(object sessionObj, ExitMsg exitMsg) {
            Session session = (Session)sessionObj;

            try {
                await session.Sockets
                    .First(c => c.Key != (exitMsg.Session.ClientId ?? Guid.NewGuid()))
                        .Value.SendAsync(
                            new NetworkMessage {
                                IncomingOpCode = ExitMsg.OpCode,
                                IncomingRecord = new ExitMsg {
                                    Session = exitMsg.Session
                                }.EncodeAsImmutable()
                            }.Encode(),
                            WebSocketMessageType.Binary,
                            true,
                            CancellationToken.None
                        );
            } catch (Exception ex) {
                Console.WriteLine(ex.ToString());
            }

        }
    }
}