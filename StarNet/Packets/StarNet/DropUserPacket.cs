using System;
using System.IO;
using System.Net;

namespace StarNet.Packets.StarNet
{
    // Used to drop users over localhost, not sent by other nodes.
    public class DropUserPacket : StarNetPacket
    {
        public string AccountName { get; set; }

        public override void Read(BinaryReader stream)
        {
            AccountName = stream.ReadString();
        }

        public override void Write(BinaryWriter stream)
        {
            stream.Write(AccountName);
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