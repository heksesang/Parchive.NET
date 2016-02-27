using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parchive.Library.PAR2.Packets
{
    /// <summary>
    /// The checksums for an input file slice.
    /// </summary>
    public struct InputFileSliceChecksum
    {
        #region Properties
        /// <summary>
        /// The MD5 checksum of the slice.
        /// </summary>
        public byte[] MD5;

        /// <summary>
        /// The CRC32 checksum of the slice.
        /// </summary>
        public uint CRC32;
        #endregion
    }

    /// <summary>
    /// A PAR2 input file slice checksum packet.
    /// </summary>
    [Packet(0x00302E3220524150, 0x0000000043534649)]
    public class InputFileSliceChecksumPacket : Packet
    {
        #region Properties
        /// <summary>
        /// The File ID of the file.
        /// </summary>
        public FileID FileID { get; set; } = new FileID();

        /// <summary>
        /// MD5 Hash and CRC32 pairs for the input file slices.
        /// </summary>
        public List<InputFileSliceChecksum> Checksums = new List<InputFileSliceChecksum>();
        #endregion

        #region Methods
        /// <summary>
        /// Initializes the packet from a stream.
        /// </summary>
        /// <param name="reader">The reader that provides access to the stream.</param>
        protected override void Initialize(BinaryReader reader)
        {
            FileID = new FileID { ID = reader.ReadBytes(16) };

            while (reader.BaseStream.Position < _Offset + _Length)
            {
                InputFileSliceChecksum checksum = new InputFileSliceChecksum();
                checksum.MD5 = reader.ReadBytes(16);
                checksum.CRC32 = reader.ReadUInt32();

                Checksums.Add(checksum);
            }
        }

        /// <summary>
        /// Writes this packet to a stream through a <see cref="BinaryWriter"/> object.
        /// </summary>
        /// <param name="writer">The <see cref="BinaryWriter"/> object.</param>
        protected override void Write(BinaryWriter writer)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
