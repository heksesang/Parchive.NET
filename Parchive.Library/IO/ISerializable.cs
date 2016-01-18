using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parchive.Library.IO
{
    /// <summary>
    /// Defines methods for serializing and deserializing the object.
    /// </summary>
    public interface ISerializable
    {
        /// <summary>
        /// Serializes the object.
        /// </summary>
        /// <returns>A byte array containing the object data.</returns>
        byte[] Serialize();

        /// <summary>
        /// Deserializes the object.
        /// </summary>
        /// <param name="input">A byte array containing the object data.</param>
        void Deserialize(byte[] input);
    }
}
