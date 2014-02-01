using System;
using StarNet.Packets;
using StarNet.Packets.Starbound;
using NHibernate.Linq;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace StarNet.ClientHandlers
{
    internal static class LoginHandlers
    {
        public static void Register()
        {
            PacketReader.RegisterPacketHandler(ClientConnectPacket.Id, HandleClientConnect);
            PacketReader.RegisterPacketHandler(HandshakeResponsePacket.Id, HandleHandshakeResponse);
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
                var random = RandomNumberGenerator.Create();
                var rawSalt = new byte[24];
                random.GetBytes(rawSalt);
                var salt = Convert.ToBase64String(rawSalt);
                client.ExpectedHash = GenerateHash(user.AccountName, user.Password, salt, 5000);
                Console.WriteLine("Expecting {0} from {1}", client.ExpectedHash, client.Socket.RemoteEndPoint);
                client.PacketQueue.Enqueue(new HandshakeChallengePacket(salt));
                client.FlushPackets();
            }
        }

        public static void HandleHandshakeResponse(StarNetNode node, StarboundClient client, IStarboundPacket _packet)
        {
            var packet = (HandshakeResponsePacket)_packet;
            if (packet.PasswordHash != client.ExpectedHash)
            {
                client.PacketQueue.Enqueue(new ConnectionResponsePacket("Incorrect login, please try again."));
                client.FlushPackets();
                node.DropClient(client);
                return;
            }
            client.ClientId = node.AllocateClientId();
            client.PacketQueue.Enqueue(new ConnectionResponsePacket(client.ClientId));
            client.FlushPackets();
            node.DropClient(client);
        }

        private static string GenerateHash(string account, string password, string challenge, int rounds)
        {
            var salt = Encoding.UTF8.GetBytes(account + challenge);
            var sha256 = SHA256.Create();
            byte[] hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            while (rounds > 0)
            {
                sha256.Initialize();
                sha256.TransformBlock(hash, 0, hash.Length, null, 0);
                sha256.TransformFinalBlock(salt, 0, salt.Length);
                hash = sha256.Hash;
                rounds--;
            }
            return Convert.ToBase64String(hash);
        }
    }
}