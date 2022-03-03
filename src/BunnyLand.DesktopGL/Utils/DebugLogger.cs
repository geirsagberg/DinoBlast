using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Text.Json;
using System.Threading.Tasks;
using Newtonsoft.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace BunnyLand.DesktopGL.Utils;

public class DebugLogger
{
    private readonly ConcurrentQueue<string> objects = new();
    private readonly NamedPipeClientStream pipeClient;
    private readonly StreamWriter writer;
    private volatile bool isConnecting;
    private volatile bool isFlushing;
    private readonly JsonSerializerOptions jsonSerializerOptions = new() { WriteIndented = true };

    public DebugLogger()
    {
        pipeClient = new NamedPipeClientStream(".", "bunnyland", PipeDirection.InOut, PipeOptions.Asynchronous);
        writer = new StreamWriter(pipeClient);
    }

    public void AddObject(object obj)
    {
        if (!pipeClient.IsConnected || isFlushing) return;
        objects.Enqueue(JsonSerializer.Serialize(obj, jsonSerializerOptions));
    }

    public void Flush()
    {
        if (isFlushing) return;

        if (pipeClient.IsConnected) {
            isFlushing = true;
            Task.Run(async () => {
                try {
                    await writer.WriteLineAsync("START");

                    while (objects.TryDequeue(out var json)) {
                        await writer.WriteLineAsync(json);
                    }

                    await writer.WriteLineAsync("END");
                } catch (Exception e) {
                    Console.WriteLine(e);
                } finally {
                    isFlushing = false;
                }
            });
        } else if (!isConnecting) {
            isConnecting = true;
            Task.Run(async () => {
                try {
                    await pipeClient.ConnectAsync();
                    var reader = new StreamReader(pipeClient);
                    var line = await reader.ReadLineAsync();
                    Console.WriteLine(line);
                    await writer.WriteLineAsync($"Client process {Process.GetCurrentProcess().Id} connected");
                } catch (Exception e) {
                    Console.WriteLine(e);
                } finally {
                    isConnecting = false;
                }
            });
        }
    }
}
