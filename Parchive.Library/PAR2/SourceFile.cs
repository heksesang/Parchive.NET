using Parchive.Library.PAR2.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parchive.Library.PAR2
{
    /// <summary>
    /// Metadata of a source file.
    /// </summary>
    public class SourceFile
    {
        #region Properties
        /// <summary>
        /// The size of the file in bytes.
        /// </summary>
        long Length { get; set; }
        #endregion

        #region Constructors
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="packet">The File Description packet.</param>
        internal SourceFile(FileDescriptionPacket packet)
        {
            Length = packet.Length;
        } 
        #endregion
    }
}
