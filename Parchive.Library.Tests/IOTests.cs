using Microsoft.VisualStudio.TestTools.UnitTesting;
using Parchive.Library.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using Parchive.Library;

namespace Parchive.Library.Tests
{
    [TestClass]
    public class IOTests
    {
        const string TestDir = @"E:\VK5v0qUjGhM0SvLLqYOfRjXY043XG04JVYRKjmKYj";
        const string TestFile = @"VK5v0qUjGhM0SvLLqYOfRjXY043XG04JVYRKjmKYj.vol001+02.par2";

        [TestMethod]
        public void CreateRecoveryFileFromRelativeUri()
        {
            Uri uri = new Uri(TestFile, UriKind.Relative);

            var r = RecoveryFile.FromUri(uri);

            Assert.AreEqual(false, r.Location.IsAbsoluteUri);
            Assert.AreEqual(
                uri,
                r.Location);
            Assert.AreEqual(
                new Range<long> { Minimum = 1, Maximum = 2 },
                r.Exponents);
        }

        [TestMethod]
        public void CreateRecoveryFileFromAbsoluteUri()
        {
            Uri uri = new Uri(new Uri(TestDir), TestFile);

            var r = RecoveryFile.FromUri(uri);

            Assert.AreEqual(true, r.Location.IsAbsoluteUri);
            Assert.AreEqual(
                uri,
                r.Location);
            Assert.AreEqual(
                new Range<long> { Minimum = 1, Maximum = 2 },
                r.Exponents);
        }

        [TestMethod]
        public void LoadRecoverySetWithAbsolutePaths()
        {
            var parFiles = Directory.EnumerateFiles(TestDir)
                .Select(x => new FileInfo(x))
                .Where(x => x.Extension == ".par2")
                .Select(x => RecoveryFile.FromUri(new Uri(x.FullName)))
                .GroupBy(x => x.Name)
                .FirstOrDefault();

            Assert.AreEqual(true, parFiles.First().Location.IsAbsoluteUri);
            Assert.AreEqual(TestDir, new FileInfo(parFiles.First().Location.AbsolutePath).DirectoryName);
        }

        [TestMethod]
        public void LoadRecoverySetWithRelativePaths()
        {
            Environment.CurrentDirectory = TestDir;
            var parFiles = Directory.EnumerateFiles(TestDir)
                .Select(x => new FileInfo(x))
                .Where(x => x.Extension == ".par2")
                .Select(x => RecoveryFile.FromUri(new Uri(x.Name, UriKind.Relative)))
                .GroupBy(x => x.Name)
                .FirstOrDefault();

            Assert.AreEqual(true, parFiles.First().Location.IsAbsoluteUri);
            Assert.AreEqual(TestDir, new FileInfo(parFiles.First().Location.AbsolutePath).DirectoryName);
        }

        [TestMethod]
        public void LoadRecoverySetWithNonExistingRelativePaths()
        {
            Environment.CurrentDirectory = TestDir + @"\..";
            var parFiles = Directory.EnumerateFiles(TestDir)
                .Select(x => new FileInfo(x))
                .Where(x => x.Extension == ".par2")
                .Select(x => RecoveryFile.FromUri(new Uri(x.Name, UriKind.Relative)))
                .GroupBy(x => x.Name)
                .FirstOrDefault();

            Assert.AreEqual(false, parFiles.First().Location.IsAbsoluteUri);
        }
    }
}
