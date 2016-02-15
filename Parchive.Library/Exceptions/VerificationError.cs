﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parchive.Library.Exceptions
{
    public class VerificationError : Exception
    {
        #region Constructors
        public VerificationError(string message) : base(message) { }
        public VerificationError() { }
        #endregion
    }
}
