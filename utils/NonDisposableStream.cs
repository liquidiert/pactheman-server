using System.Net.Sockets;
using System;

namespace pactheman_server {
    public class NonDisposableStream : NetworkStream {

        private bool _disposed = false;
        public bool YouMayDispose = false;
        public NonDisposableStream(Socket other) : base(other) {}

        protected override void Dispose(bool disposing) {
            if (_disposed) return;

            if (disposing && YouMayDispose) {
                Console.WriteLine("Got disposed :(");
                base.Dispose(disposing);
            }

            _disposed = true;
        }
    }
}