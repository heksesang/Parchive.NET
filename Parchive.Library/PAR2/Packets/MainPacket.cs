using Parchive.Library.Exceptions;
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
    internal class MainPacket : Packet
    {
        #region Fields
        private long _SliceSize;
        #endregion

        #region Properties

        /// <summary>
        /// The size of slices. Must be a positive number and a multiple of 4.
        /// </summary>
        /// <exception cref="Parchive.Library.Exceptions.InvalidSliceSizeError">
        /// The slice size is less than zero or isn't a multiple of 4.
        /// </exception>
        public long SliceSize
        {
            get
            {
                return _SliceSize;
            }
            set
            {
                if (value < 0 || value % 4 != 0)
                {
                    throw new InvalidSliceSizeError(value);
                }

                _SliceSize = value;
            }
        }

        /// <summary>
        /// Number of files in the recovery set.
        /// </summary>
        public uint NumFiles
        {
            get
            {
                return (uint)RecoveryFileIDs.Count;
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

        #region Methods
        /// <summary>
        /// Initializes the packet from a stream.
        /// </summary>
        /// <param name="reader">The reader that provides access to the stream.</param>
        /// <exception cref="Parchive.Library.Exceptions.TooLargeNumberError">
        /// The slice size is too large.
        /// </exception>
        protected override void Initialize(BinaryReader reader)
        {
            if ((SliceSize = reader.ReadInt64()) < 0)
            {
                throw new TooLargeNumberError();
            }

            var numFiles = reader.ReadUInt32();

            for (var i = 0; i < numFiles; ++i)
            {
                byte[] fileId = reader.ReadBytes(16);
                RecoveryFileIDs.Add(fileId);
            }

            while (reader.BaseStream.Position < (_Offset + _Length))
            {
                byte[] fileId = reader.ReadBytes(16);
                NonRecoveryFileIDs.Add(fileId);
            }
        }
        #endregion
    }
}
