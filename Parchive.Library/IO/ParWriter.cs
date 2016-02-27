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
    /// Writes PAR2 packets to a stream.
    /// </summary>
    public class ParWriter : BinaryWriter
    {
        #region Fields
        private IImmutableDictionary<long, long> _Packets = ImmutableDictionary<long, long>.Empty;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="ParReader"/> class based on the specified stream and using ASCII encoding.
        /// </summary>
        /// <param name="output">The output stream.</param>
        public ParWriter(Stream output) : this(output, false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ParReader"/> class based on the specified stream and using ASCII encoding.
        /// </summary>
        /// <param name="output">The output stream.</param>
        /// <param name="leaveOpen">true to leave the stream open after the <see cref="ParWriter"/> object is disposed; otherwise, false.</param>
        public ParWriter(Stream output, bool leaveOpen) : base(output, Encoding.ASCII, leaveOpen)
        {
            var origin = BaseStream.Position;
            BaseStream.Seek(0, SeekOrigin.Begin);

            using (var reader = new BinaryReader(output, Encoding.ASCII, true))
            {
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
                                var ok = Verify(reader);
                                var end = BaseStream.Position;

                                if (ok)
                                {
                                    BaseStream.Seek(pos + 8, SeekOrigin.Begin);
                                    _Packets = _Packets.Add(pos, reader.ReadInt64());
                                    BaseStream.Seek(end, SeekOrigin.Begin);
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
            }

            BaseStream.Seek(origin, SeekOrigin.Begin);
        }
        #endregion

        #region Methods
        /// <summary>
        /// Verifies the integrity of the PAR2 packet data in <see cref="BinaryWriter.BaseStream"/>.
        /// </summary>
        /// <param name="reader">The <see cref="BinaryReader"/>.</param>
        /// <returns>true if the calculated MD5 hash of the packet data is equal to the hash specified in the packet header; otherwise, false.</returns>
        private bool Verify(BinaryReader reader)
        {
            var length = reader.ReadInt64() - 32;
            var hash = reader.ReadBytes(16);

            using (var md5 = MD5.Create())
            {
                var readCount = 0;

                while (readCount < length && BaseStream.Position < BaseStream.Length)
                {
                    var bytesToRead = (int)Math.Min(length, int.MaxValue);
                    var buffer = reader.ReadBytes(bytesToRead);
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
        /// Protected default constructor.
        /// </summary>
        protected ParWriter()
        {
        }

        /// <summary>
        /// Writes a <see cref="Packet"/> object to the currrent stream.
        /// </summary>
        /// <param name="packet">The <see cref="Packet"/> object.</param>
        void Write(Packet packet)
        {
            var range = new Range<long>();
            range.Minimum = BaseStream.Position;
            
            Packet.DefaultFactory.ToStream(BaseStream, packet);
            range.Maximum = BaseStream.Position - 1;

            var overwrittenPackets = _Packets
                .Where(x => range.IsInsideRange(new Range<long>
                {
                    Minimum = x.Key,
                    Maximum = x.Key + x.Value
                }))
                .Select(x => x.Key);

            foreach (var key in overwrittenPackets)
            {
                _Packets = _Packets.Remove(key);
            }
        }
        #endregion
    }
}
