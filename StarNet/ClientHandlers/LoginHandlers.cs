using System;
using StarNet.Packets;
using StarNet.Packets.Starbound;
using NHibernate.Linq;
using System.Linq;

namespace StarNet.ClientHandlers
{
    internal static class LoginHandlers
    {
        public static void Register()
        {
            PacketReader.RegisterPacketHandler(ClientConnectPacket.Id, HandleClientConnect);
        }

        public static void HandleClientConnect(StarNetNode node, StarboundClient client, IStarboundPacket _packet)
        {
            var packet = (ClientConnectPacket)_packet;
            var guid = new Guid(packet.UUID);
            Console.WriteLine("{0} ({1}) logged in from {2} as {3}", packet.Account, guid, client.Socket.RemoteEndPoint, packet.PlayerName);
            client.PlayerName = packet.PlayerName;
            client.Account = packet.Account;
            client.UUID = guid;
            if (string.IsNullOrEmpty(client.Account))
            {
                client.PacketQueue.Enqueue(new ConnectionResponsePacket("Please log in with your StarNet account.\nNew users, register at http://starnet.io"));
                client.FlushPackets();
                node.DropClient(client);
                return;
            }
            User user;
            using (var session = node.Database.SessionFactory.OpenSession())
            {
                user = session.Query<User>().SingleOrDefault(u => u.AccountName == client.Account);
                if (user == null)
                {
                    client.PacketQueue.Enqueue(new ConnectionResponsePacket("Couldn't find your account.\nPlease try again, or\nregiser at http://starnet.io"));
                    client.FlushPackets();
                    node.DropClient(client);
                    return;
                }
                var character = user.Characters.SingleOrDefault(c => c.UUID == client.UUID);
                if (character == null)
                {
                    // TODO: Create a new character for this user
                }
                client.PacketQueue.Enqueue(new ConnectionResponsePacket("Found your account."));
                client.FlushPackets();
                node.DropClient(client);
            }
        }
    }
}