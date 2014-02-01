using System;
using StarNet.Packets;
using StarNet.Common;

namespace StarNet.Packets.Starbound
{
    public class ProtocolVersionPacket : IStarboundPacket
    {
        public static readonly byte Id = 0;
        public byte PacketId { get { return Id; } }

        public uint ProtocolVersion { get; set; }

        public ProtocolVersionPacket(uint protocolVersion)
        {
            ProtocolVersion = protocolVersion;
        }

        public void Read(StarboundStream stream)
        {
            ProtocolVersion = stream.ReadUInt32();
        }

        public void Write(StarboundStream stream)
        {
            stream.WriteUInt32(ProtocolVersion);
        }
    }
}

