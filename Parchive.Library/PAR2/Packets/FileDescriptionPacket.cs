using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parchive.Library.PAR2.Packets
{
    /// <summary>
    /// A PAR2 file description packet.
    /// </summary>
    [Packet(0x00302E3220524150, 0x63736544656C6946)]
    public class FileDescriptionPacket : Packet
    {
        #region Properties
        /// <summary>
        /// The File ID is calculated as the MD5 Hash of the last 3 fields of the body of this packet.
        /// </summary>
        public byte[] FileID { get; set; } = new byte[16];

        /// <summary>
        /// The MD5 hash of the entire file.
        /// </summary>
        public byte[] Hash { get; set; } = new byte[16];

        /// <summary>
        /// The MD5-16k. That is, the MD5 hash of the first 16kB of the file.
        /// </summary>
        public byte[] Hash16k { get; set; } = new byte[16];

        /// <summary>
        /// The length of the entire file.
        /// </summary>
        public UInt64 Length { get; set; } = 0;

        /// <summary>
        /// Name of the file.
        /// Subdirectories are indicated by an HTML-style '/' (a.k.a. the UNIX slash).
        /// The filename must be unique.
        /// </summary>
        public string Filename { get; set; } = string.Empty;
        #endregion

        #region Methods
        protected override void Initialize()
        {
            FileID = _Reader.ReadBytes(16);
            Hash = _Reader.ReadBytes(16);
            Hash16k = _Reader.ReadBytes(16);
            Length = _Reader.ReadUInt64();

            StringBuilder sb = new StringBuilder();

            while (_Reader.BaseStream.Position < _Offset + _Length)
            {
                sb.Append(_Reader.ReadChars(4));
            }

            Filename = sb.ToString();
        }

        public override bool ShouldVerifyOnInitialize()
        {
            return true;
        }
        #endregion
    }
}
