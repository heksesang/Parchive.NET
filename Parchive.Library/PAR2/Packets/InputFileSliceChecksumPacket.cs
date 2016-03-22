using System;
using System.Collections.Generic;
using System.Collections.Immutable;
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
        public ImmutableList<InputFileSliceChecksum> Checksums { get; set; } = ImmutableList.Create<InputFileSliceChecksum>();

        /// <summary>
        /// The packet body in the form of a <see cref="Stream"/> object.
        /// </summary>
        public override Stream Body
        {
            get
            {
                throw new NotImplementedException();
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Initializes the packet from a stream through a <see cref="Stream"/>.
        /// </summary>
        /// <param name="stream">A <see cref="Stream"/> containing the packet.</param>
        protected override void Initialize(Stream stream)
        {
            using (var reader = new BinaryReader(stream, Encoding.ASCII, true))
            {
                FileID = new FileID { ID = reader.ReadBytes(16) };

                while (reader.BaseStream.Position < reader.BaseStream.Length)
                {
                    Checksums.Add(new InputFileSliceChecksum
                    {
                        MD5 = reader.ReadBytes(16),
                        CRC32 = reader.ReadUInt32()
                    });
                }
            }
        }
        #endregion
    }
}
