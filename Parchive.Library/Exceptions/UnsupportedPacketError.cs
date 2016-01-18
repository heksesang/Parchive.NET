using Parchive.Library.PAR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parchive.Library.Exceptions
{
    public class UnsupportedPacketError : Exception
    {
        #region Properties
        public readonly PacketType Type;
        #endregion

        #region Constructors
        public UnsupportedPacketError(PacketType type)
        {
            Type = type;
        }
        #endregion
    }
}
