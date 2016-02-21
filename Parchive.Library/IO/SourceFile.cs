using Parchive.Library.PAR2;
using Parchive.Library.PAR2.Packets;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Parchive.Library.IO
{
    /// <summary>
    /// Contains metadata of a source file.
    /// </summary>
    public class SourceFile : IEquatable<Stream>
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
        public Uri Location { get; set; }
        
        /// <summary>
        /// The size of the file in bytes.
        /// </summary>
        public long Length { get; set; }

        /// <summary>
        /// The MD5 hash of the first 16 kb of data in the file.
        /// </summary>
        public byte[] Hash16k { get; set; }
        #endregion

        #region Methods
        /// <summary>
        /// Compares the MD5 hash of the first 16 kb of data from `source` with the `Hash16k` property.
        /// </summary>
        /// <param name="source">A stream to the source file to compare with.</param>
        /// <returns>true if the hashes are equal; otherwise false.</returns>
        public bool Equals(Stream source)
        {
            byte[] buffer = new byte[16384];
            source.Read(buffer, 0, buffer.Length);

            using (var hash16k = MD5.Create())
            {
                hash16k.TransformFinalBlock(buffer, 0, buffer.Length);
                return hash16k.Hash.SequenceEqual(Hash16k);
            }
        }
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
            Location = new Uri(Filename, UriKind.Relative);
            Length = packet.Length;
        } 
        #endregion
    }
}
