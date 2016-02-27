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
    /// Provides methods for building <see cref="Stream"/> objects.
    /// </summary>
    public static class StreamFactory
    {
        /// <summary>
        /// Available protocol handlers.
        /// </summary>
        private static readonly Dictionary<string, IProtocolHandler> _Handlers;

        /// <summary>
        /// Registers the available protocol handlers.
        /// </summary>
        static StreamFactory()
        {
            var types = typeof(IProtocolHandler).Assembly.GetTypes()
                .Where(x => x.IsClass && !x.IsAbstract && typeof(IProtocolHandler).IsAssignableFrom(x));

            _Handlers = types
                .Select(x => Activator.CreateInstance(x) as IProtocolHandler)
                .ToDictionary(x => x.SupportedProtocol);
        }

        /// <summary>
        /// Gets a resource.
        /// </summary>
        /// <param name="uri">An absolute URI to the resource.</param>
        /// <returns>A <see cref="Stream"/> object.</returns>
        public static Stream GetContent(Uri uri)
        {
            if (!_Handlers.ContainsKey(uri.Scheme))
            {
                throw new UnsupportedSchemeError(uri.Scheme);
            }

            return _Handlers[uri.Scheme].GetContent(uri);
        }

        /// <summary>
        /// Gets a resource as an asynchronous operation.
        /// </summary>
        /// <param name="uri">An absolute URI to the resource.</param>
        /// <returns>A <see cref="Stream"/> object.</returns>
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
