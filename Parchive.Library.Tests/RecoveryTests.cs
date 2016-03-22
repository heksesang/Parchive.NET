using Microsoft.VisualStudio.TestTools.UnitTesting;
using Parchive.Library.IO;
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
        const string TestDir = @"E:\VK5v0qUjGhM0SvLLqYOfRjXY043XG04JVYRKjmKYj";

        [TestMethod]
        public void CreateRecoveryData()
        {
            var matrix = new RecoveryMatrix(new Uri("file://E:/VK5v0qUjGhM0SvLLqYOfRjXY043XG04JVYRKjmKYj/"), "test", new[]
            {
                new SourceFile
                {
                     Location = new Uri(@"E:\VK5v0qUjGhM0SvLLqYOfRjXY043XG04JVYRKjmKYj\VK5v0qUjGhM0SvLLqYOfRjXY043XG04JVYRKjmKYj.part001.rar"),
                     Filename = @"VK5v0qUjGhM0SvLLqYOfRjXY043XG04JVYRKjmKYj.part001.rar"
                },
                new SourceFile
                {
                     Location = new Uri(@"E:\VK5v0qUjGhM0SvLLqYOfRjXY043XG04JVYRKjmKYj\VK5v0qUjGhM0SvLLqYOfRjXY043XG04JVYRKjmKYj.part002.rar"),
                     Filename = @"VK5v0qUjGhM0SvLLqYOfRjXY043XG04JVYRKjmKYj.part002.rar"
                },
                new SourceFile
                {
                     Location = new Uri(@"E:\VK5v0qUjGhM0SvLLqYOfRjXY043XG04JVYRKjmKYj\VK5v0qUjGhM0SvLLqYOfRjXY043XG04JVYRKjmKYj.part003.rar"),
                     Filename = @"VK5v0qUjGhM0SvLLqYOfRjXY043XG04JVYRKjmKYj.part003.rar"
                },
            }, 768000, 190);
        }
    }
}
