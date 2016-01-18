using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parchive.Library.PAR2.Packets
{
    [Packet(0x00302E3220524150, 0x0000000043534649)]
    public class InputFileSliceChecksumPacket : Packet
    {
        #region Packet Members
        protected override void Initialize(Stream input, long length)
        {
            throw new NotImplementedException();
        } 
        #endregion
    }
}
