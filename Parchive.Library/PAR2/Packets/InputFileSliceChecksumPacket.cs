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
        public UInt32 CRC32;
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
        public byte[] FileID { get; set; } = new byte[16];

        /// <summary>
        /// MD5 Hash and CRC32 pairs for the input file slices.
        /// </summary>
        public List<InputFileSliceChecksum> Checksums = new List<InputFileSliceChecksum>();
        #endregion

        #region Methods
        protected override void Initialize()
        {
            FileID = _Reader.ReadBytes(16);

            while (_Reader.BaseStream.Position < _Offset + _Length)
            {
                InputFileSliceChecksum checksum = new InputFileSliceChecksum();
                checksum.MD5 = _Reader.ReadBytes(16);
                checksum.CRC32 = _Reader.ReadUInt32();

                Checksums.Add(checksum);
            }
        }

        public override bool ShouldVerifyOnInitialize()
        {
            return true;
        }
        #endregion
    }
}
