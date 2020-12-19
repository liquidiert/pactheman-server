using System;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Collections.Concurrent;
using PacTheMan.Models;

namespace pactheman_server {
    public class SessionHandler {

        private static readonly Lazy<SessionHandler> lazy = new Lazy<SessionHandler>(() => new SessionHandler());
        public static SessionHandler Instance { get { return lazy.Value; } }
        // describes <SessionId, <ClientID, <ClientState, ClientConnection>>>
        public ConcurrentDictionary<Guid, ConcurrentDictionary<Guid, Tuple<PlayerState, TcpClient>>> MultiplayerSessions;

        private SessionHandler() {
            int numProcs = Environment.ProcessorCount;
            int concurrencyLevel = numProcs * 3;
            MultiplayerSessions = new ConcurrentDictionary<Guid, ConcurrentDictionary<Guid, Tuple<PlayerState, TcpClient>>>(concurrencyLevel, 50);
        }


    }
}