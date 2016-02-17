using Microsoft.VisualStudio.TestTools.UnitTesting;
using Parchive.Library.PAR2;
using Parchive.Library.Tasks;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace Parchive.Library.Tests
{
    [TestClass]
    public class RecoveryTests
    {
        [TestMethod]
        public void LoadRecoverySet()
        {
            var dir = @"E:\VK5v0qUjGhM0SvLLqYOfRjXY043XG04JVYRKjmKYj\";
            var set = new LocalRecoveryFileLocator().Locate(dir).FirstOrDefault();
            var src = new LocalSourceFileLocator().Locate(set).ToList();
        }

        [TestMethod]
        public void VerifySourceFiles()
        {
        }

        [TestMethod]
        public void RepairFile()
        {
        }
    }
}
