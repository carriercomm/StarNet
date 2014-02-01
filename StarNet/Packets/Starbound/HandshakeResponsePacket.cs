using System;
using StarNet.Common;

namespace StarNet.Packets.Starbound
{
    public class HandshakeResponsePacket : IStarboundPacket
    {
        public static readonly byte Id = 8;
        public byte PacketId { get { return Id; } }

        public string ClaimMessage;
        public string PasswordHash;

        public HandshakeResponsePacket()
        {
            ClaimMessage = "";
        }

        public HandshakeResponsePacket(string passwordHash) : this()
        {
            PasswordHash = passwordHash;
        }

        public void Read(StarboundStream stream)
        {
            ClaimMessage = stream.ReadString();
            PasswordHash = stream.ReadString();
        }

        public bool Write(StarboundStream stream)
        {
            stream.WriteString(ClaimMessage);
            stream.WriteString(PasswordHash);
            return false;
        }
    }
}