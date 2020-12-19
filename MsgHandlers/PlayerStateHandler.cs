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

            (client as TcpClient).Close();
        }
    }
}