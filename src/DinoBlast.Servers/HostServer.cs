using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace DinoBlast.Servers
{
    internal record Host(IPAddress Ip, short Port, string Token);

    public class HostServer
    {
        private readonly List<Host> _hosts = new();

        public void AddHost(IPAddress ip, short port, string token)
        {
            _hosts.Add(new Host(ip, port, token));
        }

        public void RemoveHost(IPAddress ip, short port, string token)
        {
            _hosts.Remove(new Host(ip, port, token));
        }

        public async Task Run(string[] args)
        {
            var app = WebApplication.Create(args);

            app.MapGet("/", async http => { await http.Response.WriteAsJsonAsync(_hosts); });

            await app.RunAsync();
        }
    }
}
