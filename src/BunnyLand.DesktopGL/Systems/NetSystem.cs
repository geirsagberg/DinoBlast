using System;
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
            FullGameState
        }

        private readonly int clientPort;
        private readonly MessageHub messageHub;
        private readonly NetManager netClient;
        private readonly NetManager netServer;
        private readonly Serializer serializer;
        private readonly int serverPort;

        private NetPeer? joinedPeer;
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
                AutoRecycle = true
            };

            var serverListener = CreateServerListener();
            netServer = new NetManager(serverListener) {
                UnconnectedMessagesEnabled = true,
                AutoRecycle = true
            };
            serverPort = gameSettings.ServerPort;
            clientPort = gameSettings.ClientPort;

            messageHub.Handle<JoinServerRequest, bool>(HandleJoinServer);
            messageHub.Handle<StartServerRequest, bool>(HandleStartServer);
            messageHub.Subscribe<StartServerSearchMessage>(OnStartServerSearch);
        }

        private void OnStartServerSearch(StartServerSearchMessage _)
        {
            netClient.Start();
            if (!netClient.SendBroadcast(new[] { (byte) NetMessageType.ListServersRequest }, serverPort)) {
                throw new Exception("Could not send broadcast");
            }
        }

        public Task<bool> HandleJoinServer(JoinServerRequest request)
        {
            joinServerTaskCompletionSource = new TaskCompletionSource<bool>();
            netClient.Start();
            joinedPeer = netClient.Connect("localhost", serverPort, "BunnyLand");
            return joinServerTaskCompletionSource.Task;
        }

        public bool HandleStartServer(StartServerRequest request)
        {
            netServer.BroadcastReceiveEnabled = true;
            return netServer.Start(serverPort);
        }

        private EventBasedNetListener CreateServerListener()
        {
            var serverListener = new EventBasedNetListener();
            serverListener.ConnectionRequestEvent += request => { request.Accept(); };
            serverListener.PeerConnectedEvent += OnPlayerJoined;
            serverListener.NetworkReceiveEvent += (peer, reader, method) => {
                Console.WriteLine("Server received: {0}", reader.GetString(100));
                reader.Recycle();
            };
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
            clientListener.ConnectionRequestEvent += request => { request.Accept(); };
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
                    }
                }
            };
            clientListener.PeerConnectedEvent += peer => {
                Console.WriteLine($"Peer connected: {peer.EndPoint}");
                joinServerTaskCompletionSource?.SetResult(true);
            };
            clientListener.NetworkErrorEvent += (endPoint, error) => { Console.WriteLine("Network error: {0}", error); };
            clientListener.NetworkReceiveUnconnectedEvent += (endPoint, reader, type) => {
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
            netServer.PollEvents();
            netClient.PollEvents();
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
