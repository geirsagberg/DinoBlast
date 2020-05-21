using System;
using System.Collections.Concurrent;
using System.IO;
using System.IO.Pipes;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace BunnyLand.DesktopGL.Utils
{
    public class DebugLogger
    {
        private const int ConnectTimeout = 1000;

        private readonly object _locker = new object();

        private readonly ConcurrentQueue<string> objects = new ConcurrentQueue<string>();
        private readonly NamedPipeClientStream pipeClient;
        private readonly StreamWriter writer;
        private volatile bool isConnecting;
        private volatile bool isFlushing;

        public DebugLogger()
        {
            pipeClient = new NamedPipeClientStream(".", "bunnyland", PipeDirection.InOut, PipeOptions.Asynchronous);
            writer = new StreamWriter(pipeClient);
        }

        public bool Connect()
        {
            try {
                pipeClient.Connect(ConnectTimeout);
                return true;
            } catch (TimeoutException) {
                return false;
            }
        }

        public void AddObject(object obj)
        {
            if (!pipeClient.IsConnected || isFlushing) return;
            objects.Enqueue(JsonConvert.SerializeObject(obj, Formatting.Indented));
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
                    } finally {
                        isFlushing = false;
                    }
                });
            } else if (!isConnecting) {
                isConnecting = true;
                Task.Run(async () => {
                    try {
                        await pipeClient.ConnectAsync();
                    } finally {
                        isConnecting = false;
                    }
                });
            }
        }
    }
}
