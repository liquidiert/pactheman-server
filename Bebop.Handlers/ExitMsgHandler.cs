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
        public static async Task HandleExitMsg(object sessionObj, ExitMsg readyMsg) {
            Session session = (Session)sessionObj;

            Console.WriteLine("received exit");

            await session.clients
                .Where(c => c.Key != (readyMsg.Session.ClientId ?? Guid.NewGuid())).First()
                    .Value.Item1.GetStream().WriteAsync(
                        new NetworkMessage {
                            IncomingOpCode = ExitMsg.OpCode,
                            IncomingRecord = new ExitMsg().EncodeAsImmutable()
                        }.Encode()
                    );

        }
    }
}