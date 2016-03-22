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
        private static readonly Dictionary<string, IProtocolHandler> handlers;

        /// <summary>
        /// Registers the available protocol handlers.
        /// </summary>
        static StreamFactory()
        {
            var types = typeof(IProtocolHandler).Assembly.GetTypes()
                .Where(x => x.IsClass && !x.IsAbstract && typeof(IProtocolHandler).IsAssignableFrom(x));

            handlers = types
                .Select(x => Activator.CreateInstance(x) as IProtocolHandler)
                .ToDictionary(x => x.SupportedProtocol);
        }

        /// <summary>
        /// Gets a stream to a resource as an asynchronous operation.
        /// </summary>
        /// <param name="uri">An absolute URI to the resource.</param>
        /// <returns>A <see cref="Stream"/> object.</returns>
        public static async Task<Stream> GetContentStreamAsync(Uri uri)
        {
            if (!handlers.ContainsKey(uri.Scheme))
            {
                throw new UnsupportedSchemeError(uri.Scheme);
            }

            return await handlers[uri.Scheme].GetContentStreamAsync(uri);
        }
    }
}
