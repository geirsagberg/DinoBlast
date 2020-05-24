using System;
using System.Linq;
using System.Threading.Tasks;
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
    public class NetClientSystem : EntityUpdateSystem
    {
        private readonly GameSettings gameSettings;
        private readonly MessageHub messageHub;
        private readonly NetManager netClient;
        private readonly Serializer serializer;
        private readonly SharedContext sharedContext;
        private GameTime gameTime = new GameTime();

        private NetPeer? joinedServer;
        private TaskCompletionSource<bool>? joinServerTaskCompletionSource;

        public NetClientSystem(GameSettings gameSettings, MessageHub messageHub, Serializer serializer, SharedContext sharedContext) : base(
            Aspect.All())
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
                DisconnectTimeout = 60000
            };
            messageHub.Handle<JoinServerRequest, bool>(HandleJoinServer);
            messageHub.Subscribe<StartServerSearchMessage>(OnStartServerSearch);
            messageHub.Subscribe<InputsUpdatedMessage>(HandleSendInputs);
        }

        private void HandleSendInputs(InputsUpdatedMessage msg)
        {
            if (netClient.IsRunning) {
                var writer = new NetDataWriter();
                foreach (var (playerNumber, input) in msg.InputsByPlayerNumber) {
                    writer.Put(new InputUpdateNetMessage(playerNumber, input), serializer);
                    // TODO: Keep updated list of peers for real peer-to-peer
                    netClient.SendToAll(writer, DeliveryMethod.Sequenced);
                    writer.Reset();
                }
            }
        }

        public Task<bool> HandleJoinServer(JoinServerRequest request)
        {
            joinServerTaskCompletionSource = new TaskCompletionSource<bool>();
            StartClient();
            joinedServer = netClient.Connect(request.EndPoint.Address.ToString(), request.EndPoint.Port, "BunnyLand");
            return joinServerTaskCompletionSource.Task;
        }

        private void StartClient()
        {
            if (!netClient.IsRunning) {
                if (netClient.Start(gameSettings.ClientPort)) {
                    Console.WriteLine("Client listening at port {0}", gameSettings.ClientPort);
                    sharedContext.IsClient = true;
                } else
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
                            var state = serializer.Deserialize<FullGameState>(reader.GetRemainingBytes());
                            var ping = peer.Ping;
                            var serverUtcNow = state.UtcNow.AddMilliseconds(ping / 2.0);
                            // var utcNow = DateTime.UtcNow;
                            // var offset = utcNow - serverUtcNow;

                            // Convert serverTime to localTime by adding offset

                            Console.WriteLine($"Ping: {ping}, serverUtcNow: {serverUtcNow}");

                            if (state.ResumeAtUtc > serverUtcNow) {
                                var resumeIn = state.ResumeAtUtc - serverUtcNow;
                                sharedContext.ResumeAtGameTime = gameTime.TotalGameTime + resumeIn;
                                sharedContext.IsPaused = true;
                                sharedContext.FrameCounter = state.FrameCounter;
                                sharedContext.MyPeerId = peer.Id;
                                messageHub.Publish(new StartGameMessage(state));
                                var writer = new NetDataWriter();
                                writer.Put((byte) NetMessageType.FullGameStateAck);
                                peer.Send(writer, DeliveryMethod.ReliableOrdered);
                            } else {
                                throw new Exception($"Cannot resume at {state.ResumeAtUtc} - the moment has passed!");
                            }

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
            clientListener.PeerConnectedEvent += peer => {
                Console.WriteLine($"Peer connected: {peer.EndPoint}");
                joinServerTaskCompletionSource?.SetResult(true);
                peer.Send(new JoinGameNetMessage(1), DeliveryMethod.ReliableOrdered, serializer);
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
            this.gameTime = gameTime;
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
