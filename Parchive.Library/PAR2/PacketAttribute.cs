using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parchive.Library.PAR2
{
    /// <summary>
    /// Identifies the packet type of the class.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false)]
    public class PacketAttribute : Attribute
    {
        #region Constructors
        public PacketAttribute(long a, long b)
        {
            Type = new PacketType(a, b);
        }
        #endregion

        #region Properties
        public PacketType Type { get; private set; }
        #endregion
    }
}
