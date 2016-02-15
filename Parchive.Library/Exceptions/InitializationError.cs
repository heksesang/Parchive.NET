using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parchive.Library.Exceptions
{
    public class InitializationError : Exception
    {
        #region Constructors
        public InitializationError(string message) : base(message) { }
        public InitializationError() { } 
        #endregion
    }
}
