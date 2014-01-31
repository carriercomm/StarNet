using System;
using System.IO;
using System.Net;

namespace StarNet.Packets.StarNet
{
    // Used to add users over localhost, not sent by other nodes.
    public class AddNewUserPacket : StarNetPacket
    {
        public string AccountName { get; set; }
        public string Password { get; set; }
        public bool NetworkAdmin { get; set; }

        public override void Read(BinaryReader stream)
        {
            AccountName = stream.ReadString();
            Password = stream.ReadString();
            NetworkAdmin = stream.ReadBoolean();
        }

        public override void Write(BinaryWriter stream)
        {
            stream.Write(AccountName);
            stream.Write(Password);
            stream.Write(NetworkAdmin);
        }

        public override MessageFlags Flags
        {
            get
            {
                return MessageFlags.ConfirmationRequired;
            }
        }
    }
}