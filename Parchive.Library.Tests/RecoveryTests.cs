using Microsoft.VisualStudio.TestTools.UnitTesting;
using Parchive.Library.PAR2;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Parchive.Library.Tests
{
    [TestClass]
    public class RecoveryTests
    {
        [TestMethod]
        public void CreateRecoverySet()
        {
            RecoverySet set = new RecoverySet(4194304, "Parchive.NET");
            
            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes("test")))
            {
                set.AddFile("test.txt", stream);
            }
        }

        [TestMethod]
        public void LoadRecoverySet()
        {
            using (var fs = File.Open(@"E:\VK5v0qUjGhM0SvLLqYOfRjXY043XG04JVYRKjmKYj\VK5v0qUjGhM0SvLLqYOfRjXY043XG04JVYRKjmKYj.par2", FileMode.Open, FileAccess.Read))
            {
                var set = new RecoverySet(new[] { fs });
            }
        }
    }
}
