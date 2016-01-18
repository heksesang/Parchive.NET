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
        #region Packet Members
        protected override void Initialize(Stream input, long length)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
