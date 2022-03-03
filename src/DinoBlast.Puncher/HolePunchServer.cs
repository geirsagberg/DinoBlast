using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using LiteNetLib;

// TODO: Finne ut kvifor ein NatIntroduction over localhost fører til både ein internal og external connection

/*
 * Kladding:
 * - Token angir eit "rom" ein vil connecte til
 * - Den første som gjer ein request til eit gitt rom blir host => Blir lagt til i _hostWaitPeers
 * - Alle andre som gjer ein request til eit gitt rom blir introdusert med host
 * - Hosts i _hostWaitPeers blir foreløpig kicked 1 minutt etter at dei blei lagt til (med mindre dei gjer nye requests, som nullstiller timer)
 */

namespace DinoBlast.Puncher;

internal struct HostFlagAndToken
{
    public HostFlagAndToken(bool isHost, string token)
    {
        IsHost = isHost;
        Token = token;
    }

    public readonly bool IsHost;
    public readonly string Token;
}

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
    private static readonly TimeSpan KickTime = new TimeSpan(0, 1, 0);

    private readonly Dictionary<string, WaitPeer> _hostWaitPeers = new Dictionary<string, WaitPeer>();
    private readonly List<string> _peersToRemove = new List<string>();
    private const IPv6Mode Ipv6Mode = IPv6Mode.Disabled;

    private NetManager _puncher;

    public void OnNatIntroductionRequest(IPEndPoint localEndPoint, IPEndPoint remoteEndPoint, string token)
    {
        Console.WriteLine($"OnNatIntroductionRequest: {localEndPoint} / {remoteEndPoint} / {token}");

        // The next commented out lines form the foundation of an extension which allows the puncher to differentiate between hosts and clients
        // var hostFlagAndToken = ExtractHostFlagAndToken(token);
        // var actualToken = $"{remoteEndPoint.Address}:{remoteEndPoint.Port}#{token}";

        // Has someone already volunteered as host for this token?
        if (_hostWaitPeers.TryGetValue(token, out var wpeer))
        {
            if (wpeer.InternalAddr.Equals(localEndPoint) &&
                wpeer.ExternalAddr.Equals(remoteEndPoint))
            {
                // If the current WaitPeer sends another request, refresh his timer
                wpeer.Refresh();
                return;
            }

            Console.WriteLine("Wait peer found, sending introduction...");

            Console.WriteLine(
                "Host - i({0}) e({1})\n  ↳ Client - i({2}) e({3})",
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
        }
        else
        {
            Console.WriteLine("Wait peer created. i({0}) e({1})", localEndPoint, remoteEndPoint);
            _hostWaitPeers[token] = new WaitPeer(localEndPoint, remoteEndPoint);
        }
    }

    public void OnNatIntroductionSuccess(IPEndPoint targetEndPoint, NatAddressType type, string token)
    {
        // Ignore, since we are the server
        Console.WriteLine("OnNatIntroductionSuccess");
    }

    public void Run()
    {
        Console.WriteLine($"Starting hole punch server at port {ServerPort}...");

        // Currently we don't need any of the events that netListener can receive
        EventBasedNetListener netListener = new EventBasedNetListener();

        _puncher = new NetManager(netListener)
        {
            IPv6Enabled = Ipv6Mode,
            NatPunchEnabled = true
        };
        _puncher.Start(ServerPort);
        _puncher.NatPunchModule.Init(this);


        while (true) {
            _puncher.PollEvents();
            _puncher.NatPunchModule.PollEvents();

            DateTime nowTime = DateTime.UtcNow;

            // Check old peers
            foreach (var waitPeer in _hostWaitPeers)
            {
                if (nowTime - waitPeer.Value.RefreshTime > KickTime)
                {
                    _peersToRemove.Add(waitPeer.Key);
                }
            }

            // Remove old peers if any
            for (int i = 0; i < _peersToRemove.Count; i++)
            {
                Console.WriteLine("Kicking peer: " + _peersToRemove[i]);
                _hostWaitPeers.Remove(_peersToRemove[i]);
            }
            _peersToRemove.Clear();

            Thread.Sleep(10);
        }
    }

    private HostFlagAndToken ExtractHostFlagAndToken(string token)
    {
        // Token format should be <IsHost>(#)<ActualToken>
        var parts = token.Split("(#)");
        if (parts.Length != 2)
            throw new Exception($"Token had an unexpected format! token = {token}");

        var isHost = Convert.ToBoolean(parts.First());
        var actualToken = parts.Last();
        return new HostFlagAndToken(isHost, actualToken);
    }
}