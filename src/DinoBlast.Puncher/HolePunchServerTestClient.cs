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

        private NetManager _c1;

        public void Run(string puncherServerIp) {
            Console.WriteLine("=== HolePunch Test ===");

            EventBasedNetListener clientListener = new EventBasedNetListener();
            EventBasedNatPunchListener natPunchListener1 = new EventBasedNatPunchListener();

            clientListener.PeerConnectedEvent += peer =>
            {
                Console.WriteLine("PeerConnected: " + peer.EndPoint);
            };

            clientListener.ConnectionRequestEvent += request =>
            {
                request.AcceptIfKey(ConnectionKey);
            };

            clientListener.PeerDisconnectedEvent += (peer, disconnectInfo) =>
            {
                Console.WriteLine("PeerDisconnected: " + disconnectInfo.Reason);
                if (disconnectInfo.AdditionalData.AvailableBytes > 0)
                {
                    Console.WriteLine("Disconnect data: " + disconnectInfo.AdditionalData.GetInt());
                }
            };

            natPunchListener1.NatIntroductionSuccess += (point, addrType, token) =>
            {
                var peer = _c1.Connect(point, ConnectionKey);
                Console.WriteLine($"NatIntroductionSuccess C1. Connecting to C2: {point}, type: {addrType}, connection created: {peer != null}");
            };

            _c1 = new NetManager(clientListener)
            {
                IPv6Enabled = Ipv6Mode,
                NatPunchEnabled = true
            };
            _c1.NatPunchModule.Init(natPunchListener1);
            _c1.Start();

            _c1.NatPunchModule.SendNatIntroduceRequest(puncherServerIp, PuncherServerPort, "token1");

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
                        Console.WriteLine("C1 stopped");
                        _c1.DisconnectPeer(_c1.FirstPeer, new byte[] {1,2,3,4});
                        _c1.Stop();
                    }
                }

                _c1.NatPunchModule.PollEvents();
                _c1.PollEvents();

                Thread.Sleep(10);
            }

            _c1.Stop();
        }

    }
}
