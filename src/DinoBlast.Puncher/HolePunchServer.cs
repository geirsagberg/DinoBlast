using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using LiteNetLib;

namespace DinoBlast.Puncher
{
    internal class WaitPeer
    {
        public IPEndPoint InternalAddr { get; }
        public IPEndPoint ExternalAddr { get; }
        public DateTime RefreshTime { get; private set; }

        public void Refresh()
        {
            RefreshTime = DateTime.UtcNow;
        }

        public WaitPeer(IPEndPoint internalAddr, IPEndPoint externalAddr)
        {
            Refresh();
            InternalAddr = internalAddr;
            ExternalAddr = externalAddr;
        }
    }

    public class HolePunchServer : INatPunchListener
    {
        // Inspired by https://github.com/RevenantX/LiteNetLib/blob/master/LibSample/HolePunchServerTest.cs
        private const int ServerPort = 50010;
        private const string ConnectionKey = "test_key";
        private static readonly TimeSpan KickTime = new TimeSpan(0, 1, 0);

        private readonly Dictionary<string, WaitPeer> _waitingPeers = new Dictionary<string, WaitPeer>();
        private readonly List<string> _peersToRemove = new List<string>();
        private const IPv6Mode Ipv6Mode = IPv6Mode.Disabled;

        private NetManager _puncher;

        public void OnNatIntroductionRequest(IPEndPoint localEndPoint, IPEndPoint remoteEndPoint, string token)
        {
            Console.WriteLine($"OnNatIntroductionRequest: {localEndPoint} / {remoteEndPoint} / {token}");
            if (_waitingPeers.TryGetValue(token, out var wpeer))
            {
                if (wpeer.InternalAddr.Equals(localEndPoint) &&
                    wpeer.ExternalAddr.Equals(remoteEndPoint))
                {
                    wpeer.Refresh();
                    return;
                }

                Console.WriteLine("Wait peer found, sending introduction...");

                // found in list - introduce client and host to eachother
                Console.WriteLine(
                    "host - i({0}) e({1})\nclient - i({2}) e({3})",
                    wpeer.InternalAddr,
                    wpeer.ExternalAddr,
                    localEndPoint,
                    remoteEndPoint);

                _puncher.NatPunchModule.NatIntroduce(
                    wpeer.InternalAddr, // host internal
                    wpeer.ExternalAddr, // host external
                    localEndPoint, // client internal
                    remoteEndPoint, // client external
                    token // request token
                );

                // clear dictionary
                _waitingPeers.Remove(token);
            }
            else
            {
                Console.WriteLine("Wait peer created. i({0}) e({1})", localEndPoint, remoteEndPoint);
                _waitingPeers[token] = new WaitPeer(localEndPoint, remoteEndPoint);
            }
        }

        public void OnNatIntroductionSuccess(IPEndPoint targetEndPoint, NatAddressType type, string token)
        {
            // ignore we are server
            Console.WriteLine("OnNatIntroductionSuccess");
        }

        public void Run()
        {
            Console.WriteLine($"Starting hole punch server at port {ServerPort}...");
            EventBasedNetListener clientListener = new EventBasedNetListener();

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
            _puncher = new NetManager(clientListener)
            {
                IPv6Enabled = Ipv6Mode,
                NatPunchEnabled = true
            };
            _puncher.Start(ServerPort);
            _puncher.NatPunchModule.Init(this);

            while (true) {
                _puncher.NatPunchModule.PollEvents();

                DateTime nowTime = DateTime.UtcNow;

                // check old peers
                foreach (var waitPeer in _waitingPeers)
                {
                    if (nowTime - waitPeer.Value.RefreshTime > KickTime)
                    {
                        _peersToRemove.Add(waitPeer.Key);
                    }
                }

                // remove
                for (int i = 0; i < _peersToRemove.Count; i++)
                {
                    Console.WriteLine("Kicking peer: " + _peersToRemove[i]);
                    _waitingPeers.Remove(_peersToRemove[i]);
                }
                _peersToRemove.Clear();

                Thread.Sleep(10);
            }
        }
    }
}
