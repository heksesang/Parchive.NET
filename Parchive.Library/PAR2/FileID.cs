using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parchive.Library.PAR2
{
    public struct FileID
    {
        #region Properties
        public byte[] ID { get; set; }
        #endregion

        #region Operators
        public static bool operator ==(FileID left, FileID right)
        {
            if (left == null || right == null)
            {
                return left == right;
            }

            return left.ID.SequenceEqual(right.ID);
        }

        public static bool operator !=(FileID left, FileID right)
        {
            if (left == null || right == null)
            {
                return left != right;
            }

            return !left.ID.SequenceEqual(right.ID);
        }
        #endregion

        #region Methods
        public override bool Equals(object obj)
        {
            if (obj is FileID)
            {
                return this == (FileID)obj;
            }

            return false;
        }

        public override int GetHashCode()
        {
            if (ID == null)
                throw new InvalidOperationException();

            return ID.Sum(b => b);
        }
        #endregion
    }
}
