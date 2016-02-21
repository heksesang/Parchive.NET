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
    /// A handler for an URI scheme.
    /// </summary>
    internal interface IProtocolHandler
    {
        /// <summary>
        /// Supported scheme.
        /// </summary>
        string SupportedProtocol { get; }

        /// <summary>
        /// Gets the content.
        /// </summary>
        /// <param name="uri">An absolute URI to the content.</param>
        /// <returns>A stream to the content.</returns>
        Stream GetContent(Uri uri);

        /// <summary>
        /// Gets the content as an asynchronous operation.
        /// </summary>
        /// <param name="uri">An absolute URI to the content.</param>
        /// <returns>A stream to the content.</returns>
        Task<Stream> GetContentAsync(Uri uri);
    }

    /// <summary>
    /// An implementation of IProtocolHolder for the 'file' scheme.
    /// </summary>
    internal class FileHandler : IProtocolHandler
    {
        /// <summary>
        /// Supported scheme.
        /// </summary>
        public string SupportedProtocol
        {
            get
            {
                return "file";
            }
        }

        /// <summary>
        /// Gets the content.
        /// </summary>
        /// <param name="uri">An absolute URI to the content.</param>
        /// <returns>A stream to the content.</returns>
        public Stream GetContent(Uri uri)
        {
            return File.Open(uri.LocalPath, FileMode.Open, FileAccess.Read);
        }

        /// <summary>
        /// Gets the content as an asynchronous operation.
        /// </summary>
        /// <param name="uri">An absolute URI to the content.</param>
        /// <returns>A stream to the content.</returns>
        public Task<Stream> GetContentAsync(Uri uri)
        {
            return Task.FromResult(GetContent(uri));
        }
    }

    /// <summary>
    /// An implementation of IProtocolHolder for the 'http' scheme.
    /// </summary>
    internal class HttpHandler : IProtocolHandler
    {
        /// <summary>
        /// Supported scheme.
        /// </summary>
        public string SupportedProtocol
        {
            get
            {
                return "http";
            }
        }

        /// <summary>
        /// Gets the content.
        /// </summary>
        /// <param name="uri">An absolute URI to the content.</param>
        /// <returns>A stream to the content.</returns>
        public Stream GetContent(Uri uri)
        {
            return GetContentAsync(uri).Result;
        }

        /// <summary>
        /// Gets the content as an asynchronous operation.
        /// </summary>
        /// <param name="uri">An absolute URI to the content.</param>
        /// <returns>A stream to the content.</returns>
        public async Task<Stream> GetContentAsync(Uri uri)
        {
            return await(new HttpClient().GetStreamAsync(uri));
        }
    }
}
