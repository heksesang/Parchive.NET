using Parchive.Library.Exceptions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
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
        /// Gets the resource.
        /// </summary>
        /// <param name="uri">An absolute URI to the resource.</param>
        /// <returns>A <see cref="Stream"/> object.</returns>
        Stream GetContent(Uri uri);

        /// <summary>
        /// Gets the resource as an asynchronous operation.
        /// </summary>
        /// <param name="uri">An absolute URI to the resource.</param>
        /// <returns>A <see cref="Stream"/> object.</returns>
        Task<Stream> GetContentAsync(Uri uri);
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
        /// Gets the resource.
        /// </summary>
        /// <param name="uri">An absolute URI to the resource.</param>
        /// <returns>A <see cref="Stream"/> object.</returns>
        public Stream GetContent(Uri uri)
        {
            return File.Open(uri.LocalPath, FileMode.Open, FileAccess.Read);
        }

        /// <summary>
        /// Gets the resource as an asynchronous operation.
        /// </summary>
        /// <param name="uri">An absolute URI to the resource.</param>
        /// <returns>A <see cref="Stream"/> object.</returns>
        public Task<Stream> GetContentAsync(Uri uri)
        {
            return Task.FromResult(GetContent(uri));
        }
    }

    /// <summary>
    /// Implementation of the `http` protocol.
    /// </summary>
    internal class HttpHandler : IProtocolHandler
    {
        /// <summary>
        /// Supported protocol.
        /// </summary>
        public string SupportedProtocol
        {
            get
            {
                return "http";
            }
        }

        /// <summary>
        /// Gets the resource.
        /// </summary>
        /// <param name="uri">An absolute URI to the resource.</param>
        /// <returns>A <see cref="Stream"/> object.</returns>
        public Stream GetContent(Uri uri)
        {
            return GetContentAsync(uri).Result;
        }

        /// <summary>
        /// Gets the resource as an asynchronous operation.
        /// </summary>
        /// <param name="uri">An absolute URI to the resource.</param>
        /// <returns>A <see cref="Stream"/> object.</returns>
        public async Task<Stream> GetContentAsync(Uri uri)
        {
            return await(new HttpClient().GetStreamAsync(uri));
        }
    }
}
