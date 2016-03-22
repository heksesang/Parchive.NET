using Parchive.Library.PAR2;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Parchive.Library.IO
{
    /// <summary>
    /// Reads PAR2 packets.
    /// </summary>
    public class ParReader : BinaryReader
    {
        #region Fields
        /// <summary>
        /// Pairs of start and packet type for each packet in the file.
        /// </summary>
        private IImmutableDictionary<long, PacketType> packets = ImmutableDictionary<long, PacketType>.Empty;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="ParReader"/> class based on the specified stream and using ASCII encoding.
        /// </summary>
        /// <param name="input">The input stream.</param>
        public ParReader(Stream input) : this(input, false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ParReader"/> class based on the specified stream and using ASCII encoding, and optionally leaves the stream open.
        /// </summary>
        /// <param name="input">The input stream.</param>
        /// <param name="leaveOpen">true to leave the stream open after the <see cref="ParReader"/> object is disposed; otherwise, false.</param>
        public ParReader(Stream input, bool leaveOpen) : base(input, Encoding.ASCII, leaveOpen)
        {
            var origin = BaseStream.Position;
            BaseStream.Seek(0, SeekOrigin.Begin);

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
                            var ok = Verify();
                            var end = BaseStream.Position;

                            if (ok)
                            {
                                BaseStream.Seek(pos + 48, SeekOrigin.Begin);
                                var type = this.ReadBytes(16);
                                BaseStream.Seek(end, SeekOrigin.Begin);
                                packets = packets.Add(pos, new PacketType(type));
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
        #endregion

        #region Methods
        /// <summary>
        /// Verifies the integrity of the PAR2 packet data in <see cref="BinaryReader.BaseStream"/>.
        /// </summary>
        /// <returns>true if the calculated MD5 hash of the packet data is equal to the hash specified in the packet header; otherwise, false.</returns>
        private bool Verify()
        {
            var length = this.ReadInt64() - 32;
            var hash = this.ReadBytes(16);

            using (var md5 = MD5.Create())
            {
                var readCount = 0;

                while (readCount < length && BaseStream.Position < BaseStream.Length)
                {
                    var bytesToRead = (int)Math.Min(length, int.MaxValue);
                    var buffer = this.ReadBytes(bytesToRead);
                    readCount += buffer.Length;

                    if (readCount < length && BaseStream.Position < BaseStream.Length)
                        md5.TransformBlock(buffer, 0, buffer.Length, null, 0);
                    else
                        md5.TransformFinalBlock(buffer, 0, buffer.Length);
                }

                return md5.Hash.SequenceEqual(hash);
            }
        }

        /// <summary>
        /// Reads a PAR2 packet at the given position.
        /// </summary>
        /// <param name="position">The position in the stream.</param>
        /// <returns>A <see cref="Packet"/> object.</returns>
        private Packet ReadPacket(long position)
        {
            BaseStream.Seek(position, SeekOrigin.Begin);
            return Packet.DefaultFactory.FromStream(BaseStream);
        }

        /// <summary>
        /// Reads the next PAR2 packet.
        /// </summary>
        /// <returns>A <see cref="Packet"/> object, read from the stream;
        /// null if there are no more packets.</returns>    
        public Packet ReadPacket()
        {
            return packets.Keys.Where(x => BaseStream.Position <= x).Select(x => ReadPacket(x)).FirstOrDefault();
        }

        /// <summary>
        /// Reads the next PAR2 packet of the given packet type.
        /// </summary>
        /// <param name="type">The packet type.</param>
        /// <returns>A <see cref="Packet"/> object of the given type, read from the stream;
        /// <see cref="null"/> null if there are no more packets of the given type.</returns>
        public Packet ReadPacket(PacketType type)
        {
            return packets.Where(x => BaseStream.Position <= x.Key && x.Value == type).Select(x => ReadPacket(x.Key)).FirstOrDefault();
        }
        #endregion
    }
}
