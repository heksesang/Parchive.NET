using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parchive.Library.PAR2.Packets
{
    /// <summary>
    /// A PAR2 main packet.
    /// </summary>
    [Packet(0x00302E3220524150, 0x000000006E69614D)]
    public class MainPacket : Packet
    {
        #region Fields
        private UInt64 _SliceSize;
        #endregion

        #region Properties

        /// <summary>
        /// The size of slices. Must be a multiple of 4.
        /// </summary>
        public UInt64 SliceSize
        {
            get
            {
                return _SliceSize;
            }
            set
            {
                if (value % 4 != 0)
                {
                    throw new Exception("Invalid slice size. Must be a multiple of 4.");
                }

                _SliceSize = value;
            }
        }

        /// <summary>
        /// Number of files in the recovery set.
        /// </summary>
        public UInt32 NumFiles
        {
            get
            {
                return (UInt32)RecoveryFileIDs.Count;
            }
        }

        /// <summary>
        /// The file IDs of the recovery files.
        /// </summary>
        public List<byte[]> RecoveryFileIDs { get; set; } = new List<byte[]>();

        /// <summary>
        /// The file IDs of the non-recovery files.
        /// </summary>
        public List<byte[]> NonRecoveryFileIDs { get; set; } = new List<byte[]>();
        #endregion

        #region Packet Members
        protected override void Initialize()
        {
            SliceSize = _Reader.ReadUInt64();
            var numFiles = _Reader.ReadUInt32();

            for (var i = 0; i < numFiles; ++i)
            {
                byte[] fileId = _Reader.ReadBytes(16);
                RecoveryFileIDs.Add(fileId);
            }

            while (_Reader.BaseStream.Position < (_Offset + _Length))
            {
                byte[] fileId = _Reader.ReadBytes(16);
                NonRecoveryFileIDs.Add(fileId);
            }
        }
        #endregion
    }
}
