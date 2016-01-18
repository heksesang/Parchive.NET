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
        #region Properties
        private UInt64 _SliceSize;

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
        #endregion

        #region Packet Members
        protected override void Initialize(Stream input, long length)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
