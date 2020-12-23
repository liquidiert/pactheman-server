using Bebop.Attributes;
using Bebop.Runtime;
using PacTheMan.Models;
using System;
using System.Threading.Tasks;
using System.Net.Sockets;

namespace pactheman_server {

    [RecordHandler]
    public static class ReadyHandler {

        [BindRecord(typeof(BebopRecord<Ready>))]
        public static async Task HandleReady(object sessionObj, Ready readyMsg) {
            Session session = (Session) sessionObj;
            if (readyMsg.Session.SessionId == null) return;

            session.clients[readyMsg.Session.ClientId ?? Guid.NewGuid()].Item2.Ready = readyMsg.Ready ?? false;

            // kinda naive but we can't influence Ready directly unfortunately
            if (session?.playerOneReady.Task.IsCompleted ?? false) {
                session?.playerOneReady.TrySetResult(true);
            } else {
                session?.playerTwoReady.TrySetResult(true);
            }

        }
    }
}