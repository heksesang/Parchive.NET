using Parchive.Library.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parchive.Library.PAR2.Packets
{
    public static class PacketExtensions
    {
        public static SourceFile ToSourceFile(this FileDescriptionPacket packet)
        {
            return new SourceFile
            {
                ID = packet.FileID,
                Filename = packet.Filename.TrimEnd('\0'),
                Location = new Uri(packet.Filename.TrimEnd('\0'), UriKind.Relative),
                Length = packet.Length,
                Hash16k = packet.Hash16k
            };
        }
    }
}
