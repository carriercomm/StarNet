using System;
using System.IO;
using StarNet.Packets.StarNet;

namespace StarNet
{
    public class ShutdownPacket : StarNetPacket
    {
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