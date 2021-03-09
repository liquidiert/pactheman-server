using CommandLine;

namespace pactheman_server {
    public class Options {

        [Option('s', "start-immediately", Required = false, 
            HelpText = "Wheter the server should start listening for sessions immediately. Good for server environments with limited GUI access.",
            Default = false)]
        public bool StartImmediately { get; set; }

        [Option(Required = false, HelpText = "Ip address to use.", Default = "127.0.0.1")]
        public string Ip { get; set; }

        [Option(Required = false, HelpText = "Port to use.", Default = 5387)]
        public int Port { get; set; }
    }
}