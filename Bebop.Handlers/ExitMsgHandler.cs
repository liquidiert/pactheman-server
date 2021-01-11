using Bebop.Attributes;
using Bebop.Runtime;
using PacTheMan.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace pactheman_server {

    [RecordHandler]
    public static class ExitMsgHandler {

        [BindRecord(typeof(BebopRecord<ExitMsg>))]
        public static async Task HandleExitMsg(object sessionObj, ExitMsg exitMsg) {
            Session session = (Session)sessionObj;

            try {
                await session.clients
                .Where(c => c.Key != (exitMsg.Session.ClientId ?? Guid.NewGuid())).First()
                    .Value.GetStream().WriteAsync(
                        new NetworkMessage {
                            IncomingOpCode = ExitMsg.OpCode,
                            IncomingRecord = new ExitMsg {
                                Session = exitMsg.Session
                            }.EncodeAsImmutable()
                        }.Encode()
                    );
            } catch (Exception ex) {
                Console.WriteLine(ex.ToString());
            }

        }
    }
}