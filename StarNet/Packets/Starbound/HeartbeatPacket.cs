using System;
using StarNet.Packets;
using StarNet.Common;

namespace StarNet.Packets.Starbound
{
    public class HeartbeatPacket : IStarboundPacket
    {
        public static readonly byte Id = 46;
        public byte PacketId { get { return Id; } }

        public ulong Value { get; set; }

        public HeartbeatPacket()
        {
        }

        public HeartbeatPacket(ulong value)
        {
            Value = value;
        }

        public void Read(StarboundStream stream)
        {
            int discarded;
            Value = stream.ReadVLQ(out discarded);
        }

        public void Write(StarboundStream stream)
        {
            stream.WriteVLQ(Value);
        }
    }
}