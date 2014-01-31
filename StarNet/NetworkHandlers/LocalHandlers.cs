using System;
using System.IO;
using System.Net;
using StarNet.Packets.StarNet;

namespace StarNet.NetworkHandlers
{
    internal static class LocalHandlers
    {
        public static void Register()
        {
            InterNodeNetwork.RegisterPacketHandler(typeof(PingPacket), HandlePing);
            InterNodeNetwork.RegisterPacketHandler(typeof(ShutdownPacket), HandleShutdown);
        }

        public static void HandlePing(StarNetPacket _packet, IPEndPoint source, InterNodeNetwork network)
        {
            // Do nothing, this is handled at a lower level
        }

        public static void HandleShutdown(StarNetPacket _packet, IPEndPoint source, InterNodeNetwork network)
        {
            if (source.Address.Equals(IPAddress.Loopback))
            {
                Console.WriteLine("Shutting down node at request of {0}.", source);
                network.LocalNode.Shutdown();
            }
        }
    }
}