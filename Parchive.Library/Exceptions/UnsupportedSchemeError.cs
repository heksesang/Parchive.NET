using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parchive.Library.Exceptions
{
    public class UnsupportedSchemeError : Exception
    {
        #region Fields
        #endregion

        #region Constructors
        public UnsupportedSchemeError(string scheme) : base("Scheme '" + scheme + "' is not supported.")
        {

        }
        #endregion
    }
}
