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
        /// The MD5 hash of the first 16 kb.
        /// </summary>
        public byte[] Hash16k { get; set; }
        #endregion

        #region Methods
        /// <summary>
        /// Compares <see cref="Hash16k"/> with the MD5 hash of the first 16kb of the input stream.
        /// </summary>
        /// <param name="input">The input stream.</param>
        /// <returns>true if the hashes are equal; otherwise, false.</returns>
        public bool Equals(Stream input)
        {
            byte[] buffer = new byte[16384];
            input.Read(buffer, 0, buffer.Length);

            using (var hash16k = MD5.Create())
            {
                hash16k.TransformFinalBlock(buffer, 0, buffer.Length);
                return hash16k.Hash.SequenceEqual(Hash16k);
            }
        }

        /// <summary>
        /// Gets the content.
        /// </summary>
        /// <returns>A <see cref="Stream"/> object.</returns>
        /// <exception cref="System.InvalidOperationException">
        /// <see cref="Location"/> is not an absolute URI.
        /// </exception>
        public Stream GetContent()
        {
            return GetContentAsync().Result;
        }

        /// <summary>
        /// Gets the content as an asynchronous operation.
        /// </summary>
        /// <returns>A <see cref="Stream"/> object.</returns>
        /// <exception cref="System.InvalidOperationException">
        /// <see cref="Location"/> is not an absolute URI.
        /// </exception>
        public async Task<Stream> GetContentAsync()
        {
            return await StreamFactory.GetContentStreamAsync(Location);
        }
        #endregion

        #region Constructors
        /// <summary>
        /// Default constructor.
        /// </summary>
        public SourceFile()
        {
        }
        #endregion
    }
}
