using Microsoft.VisualStudio.TestTools.UnitTesting;
using Parchive.Library.PAR2;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

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
            string dir = @"E:\VK5v0qUjGhM0SvLLqYOfRjXY043XG04JVYRKjmKYj\";
            RecoverySet rs = null;

            foreach (var set in RecoveryFile.FromDirectory(dir))
            {
                var streams = set.Select(x => File.Open(dir + x.Filename, FileMode.Open, FileAccess.Read));

                Debug.WriteLine(set.Key);
                Debug.WriteLine("{");
                foreach (var file in set)
                {
                    if (file.Exponents != null)
                        Debug.WriteLine("\t" + file.Exponents.Minimum + "-" + file.Exponents.Maximum);
                }
                Debug.WriteLine("}");

                rs = new RecoverySet(streams);
            }
        }

        [TestMethod]
        public void RepairFile()
        {
            RecoverySet set = null;

            using (var fs = File.Open(@"E:\VK5v0qUjGhM0SvLLqYOfRjXY043XG04JVYRKjmKYj\VK5v0qUjGhM0SvLLqYOfRjXY043XG04JVYRKjmKYj.vol127+63.par2", FileMode.Open, FileAccess.Read))
            {
                set = new RecoverySet(new[] { fs });
            }
        }
    }
}
