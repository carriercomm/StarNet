using System;
using System.IO;
using System.Net;

namespace StarNet.Packets.StarNet
{
    public class ConfirmationPacket : StarNetPacket
    {
        public ConfirmationPacket()
        {
        }

        public ConfirmationPacket(uint transaction)
        {
            Transaction = Transaction;
        }

        public override void Read(BinaryReader stream)
        {
        }

        public override void Write(BinaryWriter stream)
        {
        }

        public override MessageFlags Flags
        {
            get
            {
                return MessageFlags.None;
            }
        }
    }
}