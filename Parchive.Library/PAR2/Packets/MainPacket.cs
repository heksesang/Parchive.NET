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
    public class MainPacket : Packet
    {
        #region Fields
        private long sliceSize;
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
                return sliceSize;
            }
            set
            {
                if (value < 0 || value % 4 != 0)
                {
                    throw new InvalidSliceSizeError(value);
                }

                sliceSize = value;
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

                while (reader.BaseStream.Position < reader.BaseStream.Length)
                {
                    byte[] fileId = reader.ReadBytes(16);
                    NonRecoveryFileIDs.Add(fileId);
                }
            }
        }
        #endregion
    }
}
