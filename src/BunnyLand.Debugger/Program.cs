using System.IO.Pipes;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BunnyLand.Debugger
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var pipeServer = new NamedPipeServerStream("bunnyland", PipeDirection.InOut, NamedPipeServerStream.MaxAllowedServerInstances,
                PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
            var host = CreateHostBuilder(args)
                .ConfigureServices(services => services.AddSingleton(pipeServer))
                .Build();

            pipeServer.WaitForConnection();

            host.Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); });
    }
}
