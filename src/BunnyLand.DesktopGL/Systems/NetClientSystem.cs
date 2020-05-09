using System;
using System.Threading.Tasks;
using BunnyLand.DesktopGL.Components;
using BunnyLand.DesktopGL.Enums;
using BunnyLand.DesktopGL.Messages;
using BunnyLand.DesktopGL.Models;
using BunnyLand.DesktopGL.Serialization;
using BunnyLand.DesktopGL.Services;
using LiteNetLib;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using MonoGame.Extended.Entities;
using MonoGame.Extended.Entities.Systems;

namespace BunnyLand.DesktopGL.Systems
{
    public class NetClientSystem : EntityUpdateSystem
    {
        private readonly GameSettings gameSettings;
        private readonly MessageHub messageHub;
        private readonly NetManager netClient;
        private readonly Serializer serializer;
        private readonly SharedContext sharedContext;

        private NetPeer? joinedServer;
        private TaskCompletionSource<bool>? joinServerTaskCompletionSource;

        public NetClientSystem(GameSettings gameSettings, MessageHub messageHub, Serializer serializer, SharedContext sharedContext) : base(Aspect.All(typeof(Serializable)))
        {
            this.gameSettings = gameSettings;
            this.messageHub = messageHub;
            this.serializer = serializer;
            this.sharedContext = sharedContext;

            var clientListener = CreateClientListener();
            netClient = new NetManager(clientListener) {
                UnconnectedMessagesEnabled = true,
                AutoRecycle = true,
                BroadcastReceiveEnabled = true,
            };
            messageHub.Handle<JoinServerRequest, bool>(HandleJoinServer);
            messageHub.Subscribe<StartServerSearchMessage>(OnStartServerSearch);
        }

        public Task<bool> HandleJoinServer(JoinServerRequest request)
        {
            joinServerTaskCompletionSource = new TaskCompletionSource<bool>();
            StartClient();
            joinedServer = netClient.Connect("localhost", gameSettings.ServerPort, "BunnyLand");
            return joinServerTaskCompletionSource.Task;
        }

        private void StartClient()
        {
            if (!netClient.IsRunning) {
                if (netClient.Start(gameSettings.ClientPort)) {
                    Console.WriteLine("Client listening at port {0}", gameSettings.ClientPort);
                    sharedContext.IsClient = true;
                }
                else
                    Console.WriteLine("Client not started!");
            }
        }

        private EventBasedNetListener CreateClientListener()
        {
            var clientListener = new EventBasedNetListener();
            clientListener.ConnectionRequestEvent += request => request.Accept();
            clientListener.NetworkReceiveEvent += (peer, reader, method) => {
                if (reader.TryGetByte(out var b)) {
                    var netMessageType = (NetMessageType) b;
                    // Console.WriteLine($"Received {netMessageType} from {peer.EndPoint}");
                    switch (netMessageType) {
                        case NetMessageType.FullGameState: {
                            var state = new FullGameState(serializer);
                            state.Deserialize(reader);
                            messageHub.Publish(new StartGameMessage(state));
                            break;
                        }
                        case NetMessageType.FullGameStateUpdate: {
                            var state = new FullGameState(serializer);
                            state.Deserialize(reader);
                            messageHub.Publish(new UpdateGameMessage(state.Components!));
                            break;
                        }
                    }
                }
            };
            clientListener.PeerConnectedEvent += peer => {
                Console.WriteLine($"Peer connected: {peer.EndPoint}");
                joinServerTaskCompletionSource?.SetResult(true);
            };
            clientListener.NetworkErrorEvent += (endPoint, error) => Console.WriteLine("Network error: {0} - {1}", endPoint, error);
            clientListener.PeerDisconnectedEvent += (peer, info) => {
                Console.WriteLine("Peer disconnected: {0} - {1}", peer, info);
                if (peer == joinedServer) {
                    messageHub.Publish(new ServerDisconnectedMessage());
                    netClient.Stop();
                }
            };
            clientListener.NetworkReceiveUnconnectedEvent += (endPoint, reader, type) => {
                Console.WriteLine("Client received unconnected event from: {0}", endPoint);
                if (type == UnconnectedMessageType.BasicMessage) {
                    if (reader.TryGetByte(out var b)) {
                        var netMessageType = (NetMessageType) b;
                        Console.WriteLine($"Received {netMessageType} from {endPoint}");
                        switch (netMessageType) {
                            case NetMessageType.ListServersResponse:
                                messageHub.Publish(new ServerDiscoveredMessage(endPoint));
                                break;
                        }
                    }
                }
            };
            return clientListener;
        }

        public override void Initialize(IComponentMapperService mapperService)
        {
        }

        public override void Update(GameTime gameTime)
        {
            netClient.PollEvents();
        }

        private void OnStartServerSearch(StartServerSearchMessage _)
        {
            StartClient();

            if (!netClient.SendBroadcast(new[] { (byte) NetMessageType.ListServersRequest }, gameSettings.ServerPort)) {
                throw new Exception("Could not send broadcast");
            }
        }
    }
}
