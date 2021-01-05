using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using System;
using System.Net;
using System.Threading.Tasks;

namespace pactheman_server {
    public class Program {
        public static void Main(string[] args) {
            Task sessions = CreateSessionListener(args).Listen();
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder => {
                    webBuilder.UseStartup<Startup>();
                });

        public static SessionListener CreateSessionListener(string[] args) {
            if (args.Length > 1) {
                IPAddress address;
                if (args[0] == "localhost") args[0] = "127.0.0.1";
                if (!IPAddress.TryParse(args[0], out address)) {
                    Console.WriteLine("Error: invalid ip address");
                }
                if (args.Length < 2) {
                    int port;
                    if (!int.TryParse(args[1], out port)) {
                        Console.WriteLine("Error: port must be a number");
                    }
                    return new SessionListener(address, port);
                }
                return new SessionListener(address);
            }
            return new SessionListener(IPAddress.Parse("127.0.0.1"));
        }
    }
}
