using Bebop.Runtime;
using System;
using System.IO;
using System.Net;
using System.Linq;
using PacTheMan.Models;
using System.Net.Sockets;

namespace pactheman_server {
    public class MultiplayerListener : TcpListener  {

        MultiplayerListener(IPAddress _ip, Int32 _port = 8083) : base(_ip, _port) {}

        public void Listen() {
            Start();

            Byte[] buffer = new Byte[256];
            
            try {
                while(true) {
                    AcceptTcpClientAsync().ContinueWith(client => {
                        NetworkStream stream = client.Result.GetStream();

                        while(stream.Read(buffer, 0, buffer.Length) != 0) {
                            var message = NetworkMessage.Decode(buffer);
                            BebopMirror.HandleRecord(BebopMirror.GetRecordFromOpCode(message.IncomingOpCode ?? 0), message.PlayerState.ToArray(), client.Result);
                        }

                    });
                }
            } catch(SocketException ex) {
                Console.WriteLine("SocketException: {0}", ex);
            } finally {
                Stop();
            }

        }

    }
}