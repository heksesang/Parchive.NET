using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parchive.Library.Exceptions
{
    public class PathError : Exception
    {
        #region Constructors
        public PathError(string message) : base(message) { }
        public PathError() { } 
        #endregion
    }
}
