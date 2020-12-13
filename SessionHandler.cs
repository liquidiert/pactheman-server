using System;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Collections.Concurrent;
using PacTheMan.Models;

namespace pactheman_server {
    public class SessionHandler {

        private static readonly Lazy<SessionHandler> lazy = new Lazy<SessionHandler>(() => new SessionHandler());
        public static SessionHandler Instance { get { return lazy.Value; } }

        // <sessionID, state> -> not concurrent cause only one client at a time
        public Dictionary<Guid, PlayerState> SingleplayerSessions;

        // <sessionID, <clientID, state>> -> concurrent cause multiplayer
        public ConcurrentDictionary<Guid, ConcurrentDictionary<Guid, PlayerState>> MultiplayerSessions;

        private SessionHandler() {
            int numProcs = Environment.ProcessorCount;
            int concurrencyLevel = numProcs * 3;
            SingleplayerSessions = new Dictionary<Guid, PlayerState>();
            MultiplayerSessions = new ConcurrentDictionary<Guid, ConcurrentDictionary<Guid, PlayerState>>(concurrencyLevel, 50);
        }


    }
}