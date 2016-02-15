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
            string dir = @"E:\VK5v0qUjGhM0SvLLqYOfRjXY043XG04JVYRKjmKYj\";
            
            using (var rs = RecoverySet.Open(RecoveryFile.FromDirectory(dir).FirstOrDefault()))
            {
            }
        }

        [TestMethod]
        public void VerifySourceFiles()
        {
            string dir = @"E:\VK5v0qUjGhM0SvLLqYOfRjXY043XG04JVYRKjmKYj\";

            using (var rs = RecoverySet.Open(RecoveryFile.FromDirectory(dir).FirstOrDefault()))
            {
                ManualResetEvent resetEvent = new ManualResetEvent(false);

                rs.VerificationCompleted += (sender, e) => resetEvent.Set();

                rs.Verify();

                if (!resetEvent.WaitOne(10000))
                {
                    Assert.Fail();
                }
            }
        }

        [TestMethod]
        public void RepairFile()
        {
            string dir = @"E:\VK5v0qUjGhM0SvLLqYOfRjXY043XG04JVYRKjmKYj\";
            
            using (var rs = RecoverySet.Open(RecoveryFile.FromDirectory(dir).FirstOrDefault()))
            {
                ManualResetEvent resetEvent = new ManualResetEvent(false);

                rs.VerificationCompleted += (sender, e) => resetEvent.Set();

                rs.Verify();

                if (!resetEvent.WaitOne(10000))
                {
                    Assert.Fail();
                }
            }
        }
    }
}
