using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace BunnyLand.Debugger;

public class DebugSink : IHostedService, IAsyncDisposable
{
    private static int counter;
    private readonly Dictionary<int, string> jsons = new Dictionary<int, string>();
    private readonly ConcurrentBag<NamedPipeServerStream> servers = new ConcurrentBag<NamedPipeServerStream>();

    public async ValueTask DisposeAsync()
    {
        foreach (var server in servers) {
            await server.DisposeAsync();
        }
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _ = Task.Run(async () => {
            try {
                while (!cancellationToken.IsCancellationRequested) {
                    var index = Interlocked.Increment(ref counter);
                    var pipeServer = new NamedPipeServerStream("bunnyland", PipeDirection.InOut, NamedPipeServerStream.MaxAllowedServerInstances,
                        PipeTransmissionMode.Byte, PipeOptions.Asynchronous);

                    await pipeServer.WaitForConnectionAsync(cancellationToken);

                    var writer = new StreamWriter(pipeServer);

                    await writer.WriteLineAsync(
                        $"Connected to server {index} on process {Process.GetCurrentProcess().Id} thread {Thread.CurrentThread.ManagedThreadId}");

                    servers.Add(pipeServer);

                    StartReader(pipeServer, index);
                }
            } catch (Exception e) {
                Console.WriteLine(e);
            }
        }, cancellationToken);

        return Task.CompletedTask;
    }

    private void StartReader(NamedPipeServerStream pipeServer, int index)
    {
        _ = Task.Run(async () =>
        {
            try
            {
                var streamReader = new StreamReader(pipeServer);
                Console.WriteLine(await streamReader.ReadLineAsync());
                var stringBuilder = new StringBuilder();
                while (pipeServer.IsConnected)
                {
                    var line = await streamReader.ReadLineAsync();

                    switch (line)
                    {
                        case "START":
                            stringBuilder = new StringBuilder();
                            break;
                        case "END":
                            if (stringBuilder.Length > 0)
                            {
                                jsons[index] = stringBuilder.ToString();
                                Change?.Invoke(jsons);
                            }

                            break;
                        default:
                            stringBuilder.AppendLine(line);
                            break;
                    }
                }

                jsons.Remove(index);
                Change?.Invoke(jsons);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        });
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        foreach (var server in servers) {
            server.Disconnect();
        }
        return Task.CompletedTask;
    }

    public event Action<Dictionary<int, string>> Change;
}