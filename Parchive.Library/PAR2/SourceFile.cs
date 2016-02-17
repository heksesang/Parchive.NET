using Parchive.Library.PAR2.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parchive.Library.PAR2
{
    /// <summary>
    /// Source file metadata.
    /// </summary>
    public class SourceFile
    {
        #region Properties
        /// <summary>
        /// The ID of the file.
        /// </summary>
        public FileID ID { get; set; }

        /// <summary>
        /// The original filename of the file.
        /// </summary>
        public string Filename { get; set; }

        /// <summary>
        /// The current location of the file.
        /// </summary>
        public string Location { get; set; }
        
        /// <summary>
        /// The size of the file in bytes.
        /// </summary>
        public long Length { get; set; }
        #endregion

        #region Constructors
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="packet">The File Description packet.</param>
        internal SourceFile(FileDescriptionPacket packet)
        {
            ID = packet.FileID;
            Filename = packet.Filename.TrimEnd('\0');
            Location = Filename;
            Length = packet.Length;
        } 
        #endregion
    }
}
