using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parchive.Library.Exceptions
{
    public class InvalidPacketError : Exception
    {
        #region Constructors
        public InvalidPacketError(string message) : base(message) { }
        public InvalidPacketError() { }
        #endregion
    }
}
