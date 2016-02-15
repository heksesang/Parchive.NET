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
    /// A PAR2 recovery slice packet.
    /// </summary>
    [Packet(0x00302E3220524150, 0x63696C5376636552)]
    internal class RecoverySlicePacket : Packet
    {
        #region Properties
        public uint Exponent { get; set; }
        public byte[] RecoverySlice { get; set; }
        #endregion

        #region Methods
        /// <summary>
        /// Initializes the packet from a stream.
        /// </summary>
        /// <param name="reader">The reader that provides access to the stream.</param>
        protected override void Initialize(BinaryReader reader)
        {
            Exponent = reader.ReadUInt32();

            reader.BaseStream.Seek(_Offset + 4, SeekOrigin.Begin);
            RecoverySlice = reader.ReadBytes((int)_Length - 4);
        }
        #endregion
    }
}
