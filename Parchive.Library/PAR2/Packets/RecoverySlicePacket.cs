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
        #region Properties
        public UInt32 Exponent { get; set; }
        #endregion

        #region Methods
        public byte[] GetRecoveryData()
        {
            _Reader.BaseStream.Seek(_Offset + 4, SeekOrigin.Begin);
            return _Reader.ReadBytes((int)_Length - 4);
        }
        #endregion

        #region Packet Members
        protected override void Initialize()
        {
            Exponent = _Reader.ReadUInt32();
        }
        #endregion
    }
}
