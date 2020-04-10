using System;
using System.Collections.Generic;
using System.Net;
using BunnyLand.DesktopGL.Messages;
using BunnyLand.DesktopGL.Services;
using LiteNetLib;
using LiteNetLib.Utils;
using Microsoft.Xna.Framework;
using MonoGame.Extended.Entities.Systems;

namespace BunnyLand.DesktopGL.Systems
{
    public class NetSystem : UpdateSystem
    {
        private readonly int clientPort;
        private readonly MessageHub messageHub;
        private readonly NetManager netClient;
        private readonly NetManager netServer;
        private readonly int serverPort;
        private NetPeer? joinedPeer;

        public NetSystem(GameSettings gameSettings, MessageHub messageHub)
        {
            this.messageHub = messageHub;
            var clientListener = CreateClientListener();
            netClient = new NetManager(clientListener) {
                UnconnectedMessagesEnabled = true
            };

            var serverListener = CreateServerListener();
            netServer = new NetManager(serverListener) {
                UnconnectedMessagesEnabled = true
            };
            serverPort = gameSettings.ServerPort;
            clientPort = gameSettings.ClientPort;

            messageHub.Handle<JoinServerRequest, bool>(HandleJoinServer);
            messageHub.Handle<ListServersRequest, List<IPAddress>>(HandleListServers);
            messageHub.Handle<StartServerRequest, bool>(HandleStartServer);
        }

        public bool HandleJoinServer(JoinServerRequest request)
        {
            netClient.Start();
            joinedPeer = netClient.Connect("localhost", serverPort, "BunnyLand");
            return true;
        }

        public List<IPAddress> HandleListServers(ListServersRequest request)
        {
            netClient.Start();
            var success = netClient.SendBroadcast(new byte[] {1}, serverPort);
            return new List<IPAddress>();
        }

        public bool HandleStartServer(StartServerRequest request)
        {
            netServer.BroadcastReceiveEnabled = true;
            return netServer.Start(serverPort);
        }

        private static EventBasedNetListener CreateServerListener()
        {
            var serverListener = new EventBasedNetListener();
            serverListener.ConnectionRequestEvent += request => { request.Accept(); };
            serverListener.PeerConnectedEvent += peer => {
                Console.WriteLine("Peer connected: {0}", peer.EndPoint);
                var writer = new NetDataWriter();
                writer.Put("Welcome!");
                peer.Send(writer, DeliveryMethod.ReliableOrdered);
            };
            serverListener.NetworkReceiveEvent += (peer, reader, method) => {
                Console.WriteLine("Server received: {0}", reader.GetString(100));
                reader.Recycle();
            };
            serverListener.NetworkReceiveUnconnectedEvent += (endPoint, reader, type) => { ; };
            return serverListener;
        }

        private static EventBasedNetListener CreateClientListener()
        {
            var clientListener = new EventBasedNetListener();
            clientListener.ConnectionRequestEvent += request => { request.Accept(); };
            clientListener.NetworkReceiveEvent += (peer, reader, method) => {
                Console.WriteLine("Received: {0}", reader.GetString(100));
                reader.Recycle();
            };
            clientListener.NetworkErrorEvent += (endPoint, error) => { Console.WriteLine("Network error: {0}", error); };
            clientListener.NetworkReceiveUnconnectedEvent += (endPoint, reader, type) => { ; };
            return clientListener;
        }

        public override void Update(GameTime gameTime)
        {
            netServer.PollEvents();
            netClient.PollEvents();
        }
    }
}
