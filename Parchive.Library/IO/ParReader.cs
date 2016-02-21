using Parchive.Library.PAR2;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Parchive.Library.IO
{
    public class ParReader : BinaryReader
    {
        private IDictionary<long, PacketType> _Packets = new Dictionary<long, PacketType>();

        public ParReader(Stream input) : base(input)
        {
            var origin = BaseStream.Position;

            while (BaseStream.Position < BaseStream.Length)
            {
                int b;

                do
                {
                    b = BaseStream.ReadByte();

                    if (b == 'P')
                    {
                        byte[] buffer = new byte[8];
                        BaseStream.Read(buffer, 1, 7);
                        buffer[0] = (byte)b;

                        if (Encoding.UTF8.GetString(buffer) == "PAR2\0PKT")
                        {
                            var pos = BaseStream.Position - 8;
                            var ok = Verify(BaseStream);
                            var end = BaseStream.Position;

                            if (ok)
                            {
                                BaseStream.Seek(pos + 48, SeekOrigin.Begin);
                                var type = this.ReadBytes(16);
                                BaseStream.Seek(end, SeekOrigin.Begin);
                                _Packets[pos] = new PacketType(type);
                            }
                            else
                            {
                                BaseStream.Seek(pos + 1, SeekOrigin.Begin);
                            }
                        }
                    }
                }
                while (b != 'P');
            }

            BaseStream.Seek(origin, SeekOrigin.Begin);
        }

        public ParReader(Stream input, Encoding encoding) : base(input, encoding)
        {
        }

        public ParReader(Stream input, Encoding encoding, bool leaveOpen) : base(input, encoding, leaveOpen)
        {
        }

        private bool Verify(Stream input)
        {
            var length = this.ReadInt64() - 32;
            var hash = this.ReadBytes(16);

            using (var md5 = MD5.Create())
            {
                var readCount = 0;

                while (readCount < length && input.Position < input.Length)
                {
                    var bytesToRead = (int)Math.Min(length, int.MaxValue);
                    var buffer = this.ReadBytes(bytesToRead);
                    readCount += buffer.Length;

                    if (readCount < length && input.Position < input.Length)
                        md5.TransformBlock(buffer, 0, buffer.Length, null, 0);
                    else
                        md5.TransformFinalBlock(buffer, 0, buffer.Length);
                }

                return md5.Hash.SequenceEqual(hash);
            }
        }

        public Packet GetNextPacket()
        {
            var list = _Packets.Keys.Where(x => BaseStream.Position <= x);

            if (list.Count() == 0)
                return null;

            BaseStream.Seek(list.FirstOrDefault(), SeekOrigin.Begin);
            return Packet.FromStream(BaseStream);
        }

        public Packet GetNextPacket(PacketType type)
        {
            var list = _Packets.Where(x => BaseStream.Position <= x.Key && x.Value == type);

            if (list.Count() == 0)
                return null;
            
            BaseStream.Seek(list.FirstOrDefault().Key, SeekOrigin.Begin);
            return Packet.FromStream(BaseStream);
        }
    }
}
