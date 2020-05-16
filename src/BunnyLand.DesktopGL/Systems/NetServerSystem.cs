using System;
using System.Linq;
using System.Net.Sockets;
using BunnyLand.DesktopGL.Components;
using BunnyLand.DesktopGL.Enums;
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
    public class NetServerSystem : EntityUpdateSystem
    {
        private const int LogBroadcastedBytesEveryNthFrame = 60;

        private readonly int[] broadcastedBytes = new int[LogBroadcastedBytesEveryNthFrame];
        private readonly GameSettings gameSettings;
        private readonly NetManager netServer;
        private readonly Serializer serializer;

        private byte broadcastedBytesCounter;

        private ComponentMapper<Movable> movableMapper = null!;
        private ComponentMapper<Serializable> serializableMapper = null!;
        private ComponentMapper<SpriteInfo> spriteInfoMapper = null!;
        private ComponentMapper<Transform2> transformMapper = null!;
        private readonly MessageHub messageHub;
        private IComponentMapperService mapperService = null!;

        public NetServerSystem(GameSettings gameSettings, MessageHub messageHub, Serializer serializer) : base(Aspect.All(typeof(Serializable)))
        {
            this.gameSettings = gameSettings;
            this.serializer = serializer;

            var serverListener = CreateServerListener();
            netServer = new NetManager(serverListener) {
                UnconnectedMessagesEnabled = true,
                AutoRecycle = true,
                BroadcastReceiveEnabled = true
            };

            messageHub.Handle<StartServerRequest, bool>(HandleStartServer);
            this.messageHub = messageHub;
        }

        public bool HandleStartServer(StartServerRequest request)
        {
            if (netServer.IsRunning)
                return true;

            var started = netServer.Start(gameSettings.ServerPort);
            if (started)
                Console.WriteLine("Server listening at port {0}", gameSettings.ServerPort);
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
            serverListener.PeerDisconnectedEvent += (peer, info) => {
                Console.WriteLine("Peer disconnected {0}, {1}", peer, info);
                messageHub.Publish(new PlayerLeftMessage(peer.Id));
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

            var state = FullGameState.CreateFullGameState(serializer, mapperService, ActiveEntities);

            var writer = new NetDataWriter();
            writer.Put((byte) NetMessageType.FullGameState);
            writer.Put(state);
            peer.Send(writer, DeliveryMethod.ReliableOrdered);

            messageHub.Publish(new PlayerJoinedMessage(peer.Id));
        }


        public override void Update(GameTime gameTime)
        {
            BroadcastUpdate();

            netServer.PollEvents();
        }

        private void BroadcastUpdate()
        {
            if (netServer.ConnectedPeersCount > 0) {
                // var state = FullGameState.CreateFullGameState(serializer, ActiveEntities, serializableMapper, transformMapper, movableMapper, spriteInfoMapper);
                //
                // var writer = new NetDataWriter();
                // writer.Put((byte) NetMessageType.FullGameStateUpdate);
                // writer.Put(state);
                //
                // netServer.SendToAll(writer, DeliveryMethod.ReliableSequenced);
                //
                // broadcastedBytes[broadcastedBytesCounter] = writer.Length;
                // broadcastedBytesCounter += 1;
                // broadcastedBytesCounter %= LogBroadcastedBytesEveryNthFrame;
                // if (broadcastedBytesCounter == 0) {
                //     Console.WriteLine("Broadcasted {0:N} bytes last {1} frames", broadcastedBytes.Sum(), LogBroadcastedBytesEveryNthFrame);
                // }
            }
        }

        public override void Initialize(IComponentMapperService mapperService)
        {
            this.mapperService = mapperService;
            serializableMapper = mapperService.GetMapper<Serializable>();
            transformMapper = mapperService.GetMapper<Transform2>();
            movableMapper = mapperService.GetMapper<Movable>();
            spriteInfoMapper = mapperService.GetMapper<SpriteInfo>();
        }
    }
}
