using System;
using System.Threading;
using LiteNetLib;

namespace DinoBlast.Puncher
{
    public class HolePunchServerTestClient
    {
        private const int PuncherServerPort = 50010;
        private const string ConnectionKey = "test_key";
        private const IPv6Mode Ipv6Mode = IPv6Mode.Disabled;

        private NetManager _clientNetManager;

        public void Run(string puncherServerIp) {
            Console.WriteLine("=== HolePunch Test ===");

            EventBasedNetListener clientListener = new EventBasedNetListener();
            EventBasedNatPunchListener natPunchListener1 = new EventBasedNatPunchListener();

            clientListener.PeerConnectedEvent += peer =>
            {
                Console.WriteLine("PeerConnected: " + peer.EndPoint);
                // Can now persist peer in a list, and use peer.SendWithDeliveryEvent(..)
            };

            clientListener.ConnectionRequestEvent += request =>
            {
                request.AcceptIfKey(ConnectionKey);
            };

            clientListener.PeerDisconnectedEvent += (peer, disconnectInfo) =>
            {
                Console.WriteLine($"PeerDisconnected ({peer.EndPoint}): {disconnectInfo.Reason}");
                if (disconnectInfo.AdditionalData.AvailableBytes > 0)
                {
                    Console.WriteLine("Disconnect data: " + disconnectInfo.AdditionalData.GetInt());
                }
            };

            natPunchListener1.NatIntroductionSuccess += (point, addrType, token) =>
            {
                // TODO: Consider handling that if two peers are on the same PC, and using a hole puncher on localhost, two NAT introductions are initiated:
                // One over localhost, and one over LAN

                var peer = _clientNetManager.Connect(point, ConnectionKey);
                Console.WriteLine($"NatIntroductionSuccess. Connecting to other client: {point}, type: {addrType}, connection created: {peer != null}");
                Console.WriteLine($"Received token: {token}");
            };

            _clientNetManager = new NetManager(clientListener)
            {
                IPv6Enabled = Ipv6Mode,
                NatPunchEnabled = true
            };
            _clientNetManager.NatPunchModule.Init(natPunchListener1);
            _clientNetManager.Start();

            _clientNetManager.NatPunchModule.SendNatIntroduceRequest(puncherServerIp, PuncherServerPort, "token1");

            // keep going until ESCAPE is pressed
            Console.WriteLine("Press ESC to quit");

            while (true)
            {
                if (Console.KeyAvailable)
                {
                    var key = Console.ReadKey(true).Key;
                    if (key == ConsoleKey.Escape)
                    {
                        break;
                    }
                    if (key == ConsoleKey.A)
                    {
                        Console.WriteLine("Client stopped");
                        _clientNetManager.DisconnectPeer(_clientNetManager.FirstPeer, new byte[] {1,2,3,4});
                        _clientNetManager.Stop();
                    }
                }

                _clientNetManager.NatPunchModule.PollEvents();
                _clientNetManager.PollEvents();

                Thread.Sleep(10);
            }

            _clientNetManager.Stop();
        }

    }
}
