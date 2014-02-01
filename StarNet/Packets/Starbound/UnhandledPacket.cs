using System;
using System.IO;
using StarNet.Common;

namespace StarNet.Packets.Starbound
{
    public class UnhandledPacket : IStarboundPacket
    {
        public byte PacketId { get; set; }
        public byte[] Data { get; set; }

        public UnhandledPacket(byte[] data, byte packetId)
        {
            Data = data;
            PacketId = packetId;
        }

        public void Read(StarboundStream stream)
        {
            stream.Read(Data, 0, Data.Length);
        }

        public void Write(StarboundStream stream)
        {
            stream.Write(Data, 0, Data.Length);
        }
    }
}

