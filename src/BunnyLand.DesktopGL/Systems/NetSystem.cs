using System;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;
using BunnyLand.DesktopGL.Components;
using BunnyLand.DesktopGL.Messages;
using BunnyLand.DesktopGL.Serialization;
using BunnyLand.DesktopGL.Services;
using LiteNetLib;
using LiteNetLib.Utils;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using MonoGame.Extended.Entities;
using MonoGame.Extended.Entities.Systems;

namespace BunnyLand.DesktopGL.Systems
{
    public class NetSystem : EntityUpdateSystem
    {
        public enum NetMessageType : byte
        {
            ListServersRequest,
            ListServersResponse,
            FullGameState,
            FullGameStateUpdate
        }

        private const int LogBroadcastedBytesEveryNthFrame = 60;

        private readonly int[] broadcastedBytes = new int[LogBroadcastedBytesEveryNthFrame];

        private readonly int clientPort;
        private readonly MessageHub messageHub;
        private readonly NetManager netClient;
        private readonly NetManager netServer;
        private readonly Serializer serializer;
        private readonly int serverPort;

        private byte broadcastedBytesCounter;

        private NetPeer? joinedServer;
        private TaskCompletionSource<bool> joinServerTaskCompletionSource;
        private ComponentMapper<Movable> movableMapper;
        private ComponentMapper<Serializable> serializableMapper;
        private ComponentMapper<SpriteInfo> spriteInfoMapper;
        private ComponentMapper<Transform2> transformMapper;

        public NetSystem(GameSettings gameSettings, MessageHub messageHub, Serializer serializer) : base(Aspect.All(typeof(Serializable)))
        {
            this.messageHub = messageHub;
            this.serializer = serializer;
            var clientListener = CreateClientListener();
            netClient = new NetManager(clientListener) {
                UnconnectedMessagesEnabled = true,
                AutoRecycle = true,
                BroadcastReceiveEnabled = true,
            };

            var serverListener = CreateServerListener();
            netServer = new NetManager(serverListener) {
                UnconnectedMessagesEnabled = true,
                AutoRecycle = true,
                BroadcastReceiveEnabled = true
            };
            serverPort = gameSettings.ServerPort;
            clientPort = gameSettings.ClientPort;

            messageHub.Handle<JoinServerRequest, bool>(HandleJoinServer);
            messageHub.Handle<StartServerRequest, bool>(HandleStartServer);
            messageHub.Subscribe<StartServerSearchMessage>(OnStartServerSearch);
        }

        private void OnStartServerSearch(StartServerSearchMessage _)
        {
            StartClient();

            if (!netClient.SendBroadcast(new[] { (byte) NetMessageType.ListServersRequest }, serverPort)) {
                throw new Exception("Could not send broadcast");
            }
        }

        private void StartClient()
        {
            if (!netClient.IsRunning) {
                if (netClient.Start(clientPort))
                    Console.WriteLine("Client listening at port {0}", clientPort);
                else
                    Console.WriteLine("Client not started!");
            }
        }

        public Task<bool> HandleJoinServer(JoinServerRequest request)
        {
            joinServerTaskCompletionSource = new TaskCompletionSource<bool>();
            StartClient();
            joinedServer = netClient.Connect("localhost", serverPort, "BunnyLand");
            return joinServerTaskCompletionSource.Task;
        }

        public bool HandleStartServer(StartServerRequest request)
        {
            if (netServer.IsRunning)
                return true;

            var started = netServer.Start(serverPort);
            if (started)
                Console.WriteLine("Server listening at port {0}", serverPort);
            else
                Console.WriteLine("Server not started!");
            return started;
        }

        private EventBasedNetListener CreateServerListener()
        {
            var serverListener = new EventBasedNetListener();
            serverListener.ConnectionRequestEvent += request => { request.Accept(); };
            serverListener.PeerConnectedEvent += OnPlayerJoined;
            serverListener.NetworkReceiveEvent += (peer, reader, method) => { Console.WriteLine("Server received: {0}", reader.GetString(100)); };
            serverListener.NetworkReceiveUnconnectedEvent += (endPoint, reader, type) => {
                if (endPoint.AddressFamily == AddressFamily.InterNetwork && type == UnconnectedMessageType.Broadcast) {
                    if (reader.TryGetByte(out var b)) {
                        switch ((NetMessageType) b) {
                            case NetMessageType.ListServersRequest: {
                                netServer.SendUnconnectedMessage(new[] { (byte) NetMessageType.ListServersResponse }, endPoint);
                                break;
                            }
                        }
                    }
                }
            };
            return serverListener;
        }

        private void OnPlayerJoined(NetPeer peer)
        {
            Console.WriteLine("Peer connected: {0}", peer.EndPoint);

            Console.WriteLine("Sending initial world data");

            var state = FullGameState.CreateFullGameState(serializer, ActiveEntities, serializableMapper, transformMapper, movableMapper, spriteInfoMapper);

            var writer = new NetDataWriter();
            writer.Put((byte) NetMessageType.FullGameState);
            writer.Put(state);
            peer.Send(writer, DeliveryMethod.ReliableOrdered);
        }

        private EventBasedNetListener CreateClientListener()
        {
            var clientListener = new EventBasedNetListener();
            clientListener.ConnectionRequestEvent += request => request.Accept();
            clientListener.NetworkReceiveEvent += (peer, reader, method) => {
                if (reader.TryGetByte(out var b)) {
                    var netMessageType = (NetMessageType) b;
                    Console.WriteLine($"Received {netMessageType} from {peer.EndPoint}");
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
            clientListener.PeerDisconnectedEvent += (peer, info) => Console.WriteLine("Peer disconnected: {0} - {1}", peer, info);
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

        public override void Update(GameTime gameTime)
        {
            BroadcastUpdate();

            netServer.PollEvents();
            netClient.PollEvents();
        }

        private void BroadcastUpdate()
        {
            // TODO: Real broadcast for LAN optimization with many players?
            if (netServer.ConnectedPeersCount > 0) {
                var state = FullGameState.CreateFullGameState(serializer, ActiveEntities, serializableMapper, transformMapper, movableMapper, spriteInfoMapper);

                var writer = new NetDataWriter();
                writer.Put((byte) NetMessageType.FullGameStateUpdate);
                writer.Put(state);

                netServer.SendToAll(writer, DeliveryMethod.Sequenced);

                broadcastedBytes[broadcastedBytesCounter] = writer.Length;
                broadcastedBytesCounter += 1;
                broadcastedBytesCounter %= LogBroadcastedBytesEveryNthFrame;
                if (broadcastedBytesCounter == 0) {
                    Console.WriteLine("Broadcasted {0:N} bytes last {1} frames", broadcastedBytes.Sum(), LogBroadcastedBytesEveryNthFrame);
                }
            }
        }

        public override void Initialize(IComponentMapperService mapperService)
        {
            serializableMapper = mapperService.GetMapper<Serializable>();
            transformMapper = mapperService.GetMapper<Transform2>();
            movableMapper = mapperService.GetMapper<Movable>();
            spriteInfoMapper = mapperService.GetMapper<SpriteInfo>();
        }
    }
}
