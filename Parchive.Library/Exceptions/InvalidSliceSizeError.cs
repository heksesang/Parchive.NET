using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parchive.Library.Exceptions
{
    public class InvalidSliceSizeError : Exception
    {
        #region Properties
        public readonly long SliceSize;
        #endregion

        #region Constructors
        public InvalidSliceSizeError(long sliceSize) :
            base("Invalid slice size. Must be a positive number and a multiple of 4.")
        {
            SliceSize = sliceSize;
        }
        #endregion
    }
}
