using System;
using System.Net.Sockets;
using BunnyLand.DesktopGL.Components;
using BunnyLand.DesktopGL.Enums;
using BunnyLand.DesktopGL.Extensions;
using BunnyLand.DesktopGL.Messages;
using BunnyLand.DesktopGL.Models;
using BunnyLand.DesktopGL.Serialization;
using BunnyLand.DesktopGL.Services;
using LiteNetLib;
using LiteNetLib.Utils;
using Microsoft.Xna.Framework;
using MonoGame.Extended.Entities;
using MonoGame.Extended.Entities.Systems;

namespace BunnyLand.DesktopGL.Systems
{
    public class NetServerSystem : EntityUpdateSystem
    {
        private const int LogBroadcastedBytesEveryNthFrame = 60;

        private readonly int[] broadcastedBytes = new int[LogBroadcastedBytesEveryNthFrame];
        private readonly GameSettings gameSettings;

        private readonly MessageHub messageHub;
        private readonly NetManager netServer;
        private readonly Serializer serializer;
        private readonly SharedContext sharedContext;

        private byte broadcastedBytesCounter;
        private IComponentMapperService componentMapperService = null!;

        public NetServerSystem(GameSettings gameSettings, MessageHub messageHub, Serializer serializer, SharedContext sharedContext) : base(
            Aspect.All(typeof(Serializable)))
        {
            this.gameSettings = gameSettings;
            this.serializer = serializer;
            this.sharedContext = sharedContext;

            var serverListener = CreateServerListener();
            netServer = new NetManager(serverListener) {
                UnconnectedMessagesEnabled = true,
                AutoRecycle = true,
                BroadcastReceiveEnabled = true
            };

            messageHub.Handle<StartServerRequest, bool>(HandleStartServer);
            messageHub.Handle<SendInputsRequest>(HandleSendInputs);
            this.messageHub = messageHub;
        }

        private void HandleSendInputs(SendInputsRequest request)
        {
            var inputs = request.InputsByFrame;
            var writer = new NetDataWriter();
            writer.Put(NetMessageType.ClientAction, serializer.Serialize(inputs));
            netServer.SendToAll(writer, DeliveryMethod.Sequenced);
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

            var state = FullGameState.CreateFullGameState(componentMapperService, ActiveEntities, sharedContext.FrameCounter);
            var bytes = serializer.Serialize(state);
            var writer = new NetDataWriter();
            writer.Put((byte) NetMessageType.FullGameState);
            writer.Put(bytes);
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
            componentMapperService = mapperService;
        }
    }
}
