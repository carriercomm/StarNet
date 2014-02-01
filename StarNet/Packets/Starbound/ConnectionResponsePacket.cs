using System;
using StarNet.Common;

namespace StarNet.Packets.Starbound
{
    public class ConnectionResponsePacket : IStarboundPacket
    {
        public static readonly byte Id = 1;
        public byte PacketId { get { return Id; } }

        public bool Success;
        public ulong ClientId;
        public string RejectionReason;

        public ConnectionResponsePacket()
        {
        }

        public ConnectionResponsePacket(ulong clientId)
        {
            ClientId = clientId;
            Success = true;
            RejectionReason = string.Empty;
        }

        public ConnectionResponsePacket(string rejectionReason)
        {
            Success = false;
            RejectionReason = rejectionReason;
            ClientId = 0;
        }

        public void Read(StarboundStream stream)
        {
            int discarded;
            Success = stream.ReadBoolean();
            ClientId = stream.ReadVLQ(out discarded);
            RejectionReason = stream.ReadString();
        }

        public void Write(StarboundStream stream)
        {
            stream.WriteBoolean(Success);
            stream.WriteVLQ(ClientId);
            stream.WriteString(RejectionReason);
        }
    }
}