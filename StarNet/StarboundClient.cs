using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Net;
using System.IO;
using Ionic.Zlib;
using StarNet.Packets;
using StarNet.Common;
using StarNet.Packets.Starbound;
using System.Linq;

namespace StarNet
{
    public class StarboundClient
    {
        public Socket Socket { get; set; }
        public ConcurrentQueue<IStarboundPacket> PacketQueue { get; set; }
        public StarboundServer CurrentServer { get; set; }
        public PacketReader PacketReader { get; set; }
        public byte[] Shipworld { get; set; } // TODO: Save to disk to save on memory usage
        public string PlayerName { get; set; }
        public string Species { get; set; }
        public string Account { get; set; }
        public Guid UUID { get; set; }

        internal string ExpectedHash { get; set; }

        public StarboundClient(Socket socket)
        {
            Socket = socket;
            PacketQueue = new ConcurrentQueue<IStarboundPacket>();
            PacketReader = new PacketReader();
        }

        public void FlushPackets()
        {
            while (PacketQueue.Count > 0)
            {
                IStarboundPacket next;
                while (!PacketQueue.TryDequeue(out next)) ;
                var memoryStream = new MemoryStream();
                var stream = new StarboundStream(memoryStream);
                var compressed = next.Write(stream);
                byte[] buffer = new byte[stream.Position];
                Array.Copy(memoryStream.GetBuffer(), buffer, buffer.Length);
                int length = buffer.Length;
                if (compressed)
                {
                    buffer = ZlibStream.CompressBuffer(buffer);
                    length = -buffer.Length;
                }
                byte[] header = new byte[StarboundStream.GetSignedVLQLength(length) + 1];
                header[0] = next.PacketId;
                int discarded;
                StarboundStream.WriteSignedVLQ(header, 1, length, out discarded);
                int payloadStart = header.Length;
                Array.Resize(ref header, header.Length + buffer.Length);
                Array.Copy(buffer, 0, header, payloadStart, buffer.Length);
                Socket.Send(header);
            }
        }
    }
}