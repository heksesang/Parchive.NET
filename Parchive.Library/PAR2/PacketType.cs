using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parchive.Library.PAR2
{
    public struct PacketType
    {
        #region Properties
        /// <summary>
        /// The byte array of the packet type.
        /// </summary>
        public byte[] Identifier { get; private set; }
        #endregion

        #region Constructors
        public PacketType(byte[] identifier)
        {
            Identifier = identifier;
        }

        public PacketType(UInt64 a, UInt64 b)
        {
            Identifier = new byte[16];

            BitConverter.GetBytes(a).CopyTo(Identifier, 0);
            BitConverter.GetBytes(b).CopyTo(Identifier, 8);
        }
        #endregion

        #region Operators
        public static bool operator ==(PacketType left, PacketType right)
        {
            if (left == null || right == null)
            {
                return left == right;
            }

            return left.Identifier.SequenceEqual(right.Identifier);
        }

        public static bool operator !=(PacketType left, PacketType right)
        {
            if (left == null || right == null)
            {
                return left != right;
            }

            return !left.Identifier.SequenceEqual(right.Identifier);
        }
        #endregion

        #region Comparison Methods
        public override bool Equals(object obj)
        {
            if (obj is PacketType)
            {
                return this == (PacketType)obj;
            }

            return false;
        }

        public override int GetHashCode()
        {
            if (Identifier == null)
                throw new InvalidOperationException();

            return Identifier.Sum(b => b);
        }
        #endregion
    }
}
