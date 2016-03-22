using Parchive.Library.Exceptions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Parchive.Library.IO
{
    /// <summary>
    /// Defines methods for accessing resources using a specific protocol.
    /// </summary>
    internal interface IProtocolHandler
    {
        /// <summary>
        /// Supported protocol.
        /// </summary>
        string SupportedProtocol { get; }

        /// <summary>
        /// Gets a stream to a resource as an asynchronous operation.
        /// </summary>
        /// <param name="uri">An absolute URI to the resource.</param>
        /// <returns>A <see cref="Stream"/> object.</returns>
        Task<Stream> GetContentStreamAsync(Uri uri);
    }

    /// <summary>
    /// Implementation of the `file` protocol.
    /// </summary>
    internal class FileHandler : IProtocolHandler
    {
        /// <summary>
        /// Supported protocol.
        /// </summary>
        public string SupportedProtocol
        {
            get
            {
                return "file";
            }
        }

        /// <summary>
        /// Gets a stream to a resource as an asynchronous operation.
        /// </summary>
        /// <param name="uri">An absolute URI to the resource.</param>
        /// <returns>A <see cref="Stream"/> object.</returns>
        public async Task<Stream> GetContentStreamAsync(Uri uri)
        {
            if (!File.Exists(uri.AbsolutePath))
            {
                return await Task.FromResult(File.Create(uri.AbsolutePath));
            }
            else
            {
                return await Task.FromResult(File.Open(uri.LocalPath, FileMode.Open, FileAccess.ReadWrite));
            }
        }
    }
}
