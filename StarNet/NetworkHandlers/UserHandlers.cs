using System;
using System.Net;
using StarNet.Packets.StarNet;
using NHibernate.Linq;
using System.Linq;

namespace StarNet.NetworkHandlers
{
    internal static class UserHandlers
    {
        public static void Register()
        {
            InterNodeNetwork.RegisterPacketHandler(typeof(AddNewUserPacket), HandleAddNewUser);
        }

        public static void HandleAddNewUser(StarNetPacket _packet, IPEndPoint source, InterNodeNetwork network)
        {
            var packet = (AddNewUserPacket)_packet;
            if (source.Address.Equals(IPAddress.Loopback))
            {
                using (var session = network.LocalNode.Database.SessionFactory.OpenSession())
                {
                    if (session.Query<User>().Any(u => u.AccountName == packet.AccountName))
                    {
                        Console.WriteLine("Warning: Attempted to add account that already exists.");
                        return;
                    }
                    using (var transaction = session.BeginTransaction())
                    {
                        var user = new User
                        {
                            AccountName = packet.AccountName,
                            Password = packet.Password,
                            NetworkAdmin = packet.NetworkAdmin
                        };
                        session.SaveOrUpdate(user);
                        transaction.Commit();
                    }
                    Console.WriteLine("Added user {0} at request of {1}", packet.AccountName, source);
                }
            }
        }
    }
}