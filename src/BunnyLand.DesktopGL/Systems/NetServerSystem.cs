using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using BunnyLand.DesktopGL.Enums;
using BunnyLand.DesktopGL.Extensions;
using BunnyLand.DesktopGL.Messages;
using BunnyLand.DesktopGL.Models;
using BunnyLand.DesktopGL.NetMessages;
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

        private readonly Dictionary<NetPeer, PeerStatus> statusByPeer = new Dictionary<NetPeer, PeerStatus>();

        private IComponentMapperService componentMapperService = null!;
        private GameTime gameTime = new GameTime();

        public NetServerSystem(GameSettings gameSettings, MessageHub messageHub, Serializer serializer, SharedContext sharedContext) : base(
            Aspect.All())
        {
            this.gameSettings = gameSettings;
            this.serializer = serializer;
            this.sharedContext = sharedContext;

            var serverListener = CreateServerListener();
            netServer = new NetManager(serverListener) {
                UnconnectedMessagesEnabled = true,
                AutoRecycle = true,
                BroadcastReceiveEnabled = true,
                DisconnectTimeout = 60000
            };

            messageHub.Handle<StartServerRequest, bool>(HandleStartServer);
            messageHub.Subscribe<InputsUpdatedMessage>(HandleSendInputs);
            this.messageHub = messageHub;
        }

        private void HandleSendInputs(InputsUpdatedMessage updatedMessage)
        {
            if (netServer.IsRunning) {
                var writer = new NetDataWriter();
                foreach (var (playerNumber, input) in updatedMessage.InputsByPlayerNumber) {
                    writer.Put(new InputUpdateNetMessage(playerNumber, input), serializer);
                    netServer.SendToAll(writer, DeliveryMethod.Sequenced);
                    writer.Reset();
                }
            }
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
            serverListener.PeerConnectedEvent += OnPeerConnected;
            serverListener.NetworkReceiveEvent += (peer, reader, method) => {
                // Console.WriteLine("Server received: {0}", reader.GetString(100));
                if (reader.TryGetByte(out var b)) {
                    switch ((NetMessageType) b) {
                        case NetMessageType.FullGameStateAck:
                            statusByPeer[peer] = PeerStatus.Joined;
                            break;
                        case NetMessageType.JoinGameRequest: {
                            var msg = serializer.Deserialize<JoinGameNetMessage>(reader.GetRemainingBytes());
                            OnPlayerJoining(peer, msg.PlayerCount);
                            break;
                        }
                        case NetMessageType.PlayerInputs: {
                            var msg = serializer.Deserialize<InputUpdateNetMessage>(reader.GetRemainingBytes());
                            messageHub.Publish(new ReceivedInputMessage(msg.PlayerNumber, msg.Input));
                            break;
                        }
                    }
                }
            };
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

        private void OnPeerConnected(NetPeer peer)
        {
            Console.WriteLine("Peer connected: {0}", peer.EndPoint);
        }

        private void OnPlayerJoining(NetPeer peer, byte playerCount)
        {
            statusByPeer[peer] = PeerStatus.Initial;

            sharedContext.IsPaused = true;

            messageHub.Publish(new PlayerJoinedMessage(peer.Id, playerCount));
        }


        private void SendWorldData(NetPeer peer)
        {
            var utcNow = DateTime.UtcNow;
            var resumeIn = TimeSpan.FromSeconds(1);
            var resumeAtUtc = utcNow.Add(resumeIn);
            sharedContext.ResumeAtGameTime = gameTime.TotalGameTime + resumeIn;


            Console.WriteLine("Sending initial world data");
            var state = FullGameState.CreateFullGameState(componentMapperService, ActiveEntities, sharedContext.FrameCounter, utcNow, resumeAtUtc, peer.Id);
            var bytes = serializer.Serialize(state);
            var writer = new NetDataWriter();
            writer.Put(NetMessageType.FullGameState, bytes);
            peer.Send(writer, DeliveryMethod.ReliableOrdered);
            statusByPeer[peer] = PeerStatus.WorldDataSent;
        }

        public override void Update(GameTime gameTime)
        {
            this.gameTime = gameTime;
            // BroadcastUpdate();

            foreach (var peer in statusByPeer.Where(kvp => kvp.Value == PeerStatus.Initial).Select(kvp => kvp.Key).ToList()) {
                SendWorldData(peer);
            }

            netServer.PollEvents();

            if (sharedContext.IsPaused && gameTime.TotalGameTime > sharedContext.ResumeAtGameTime
                && statusByPeer.Values.Any(v => v == PeerStatus.WorldDataSent)) {
                throw new Exception("Peers still joining; aborting to avoid desync");
            }
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

    internal enum PeerStatus
    {
        Initial,
        WorldDataSent,
        Joined
    }
}
