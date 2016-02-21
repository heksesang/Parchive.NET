using Parchive.Library.Exceptions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parchive.Library.IO
{
    /// <summary>
    /// 
    /// </summary>
    public static class StreamBuilder
    {
        private static readonly Dictionary<string, IProtocolHandler> _Handlers;

        static StreamBuilder()
        {
            var types = typeof(IProtocolHandler).Assembly.GetTypes()
                .Where(x => typeof(IProtocolHandler).IsAssignableFrom(x));

            _Handlers = types
                .Select(x => Activator.CreateInstance(x) as IProtocolHandler)
                .ToDictionary(x => x.SupportedProtocol);
        }

        /// <summary>
        /// Gets the content.
        /// </summary>
        /// <param name="uri">An absolute URI to the content.</param>
        /// <returns>A stream to the content.</returns>
        public static Stream GetContent(Uri uri)
        {
            if (!_Handlers.ContainsKey(uri.Scheme))
            {
                throw new UnsupportedSchemeError(uri.Scheme);
            }

            return _Handlers[uri.Scheme].GetContent(uri);
        }

        /// <summary>
        /// Gets the content as an asynchronous operation.
        /// </summary>
        /// <param name="uri">An absolute URI to the content.</param>
        /// <returns>A stream to the content.</returns>
        public static async Task<Stream> GetContentAsync(Uri uri)
        {
            if (!_Handlers.ContainsKey(uri.Scheme))
            {
                throw new UnsupportedSchemeError(uri.Scheme);
            }

            return await _Handlers[uri.Scheme].GetContentAsync(uri);
        }
    }
}
