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
    public class RecoverySlicePacket : Packet
    {
        #region Fields
        private uint exponent;
        private Stream stream;
        #endregion

        #region Properties
        /// <summary>
        /// The exponent used by this recovery slice.
        /// </summary>
        public uint Exponent
        {
            get
            {
                return exponent;
            }
            set
            {
                if (value <= ushort.MaxValue)
                    exponent = value;
            }
        }

        /// <summary>
        /// A <see cref="Stream"/> of recovery data.
        /// </summary>
        public Stream Stream
        {
            get
            {
                return stream;
            }
            set
            {
                stream = value;
            }
        }

        /// <summary>
        /// The packet body in the form of a <see cref="Stream"/> object.
        /// </summary>
        public override Stream Body
        {
            get
            {
                return stream;
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
                Exponent = reader.ReadUInt32();
                this.stream = stream;
            }
        }
        #endregion
    }
}
