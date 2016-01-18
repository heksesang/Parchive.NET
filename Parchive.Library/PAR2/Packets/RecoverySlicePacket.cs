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
        #region Packet Members
        protected override void Initialize(Stream input, long length)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
