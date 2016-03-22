using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Text;
using Parchive.Library.IO;

namespace Parchive.Library.Tests.IO
{
    [TestClass]
    public class ConstrainedStreamTests
    {
        private ConstrainedStream stream;
        private long length;
        private long position;

        [TestInitialize]
        public void Initialize()
        {
            var bytes = Encoding.UTF8.GetBytes("Test data");
            length = bytes.Length - 4;
            position = 0;
            stream = new ConstrainedStream(new MemoryStream(bytes), 4, length);
        }

        [TestMethod]
        public void GetLength()
        {
            Assert.AreEqual(length, stream.Length);
        }

        [TestMethod]
        public void GetPosition()
        {
            Assert.AreEqual(position, stream.Position);
        }

        [TestMethod]
        public void SeekToMiddle()
        {
            var position = stream.Seek(stream.Length / 2, SeekOrigin.Begin);

            Assert.AreEqual(stream.Position, position);
            Assert.AreEqual(stream.Length / 2, stream.Position);
        }

        [TestMethod]
        public void SeekBeyondLowerLimit()
        {
            var position = stream.Seek(stream.Length + 1, SeekOrigin.Begin);

            Assert.AreEqual(stream.Position, position);
            Assert.AreEqual(stream.Length, stream.Position);
        }

        [TestMethod]
        public void SeekBeyondUpperLimit()
        {
            var position = stream.Seek(-1, SeekOrigin.Begin);

            Assert.AreEqual(stream.Position, position);
            Assert.AreEqual(0, stream.Position);
        }

        [TestMethod]
        public void Read()
        {
            var buffer = new byte[stream.Length];
            var read = stream.Read(buffer, 0, buffer.Length);

            Assert.AreEqual(buffer.Length, read);
        }

        [TestMethod]
        public void ReadMoreThanAvailable()
        {
            var buffer = new byte[stream.Length + 1];
            var read = stream.Read(buffer, 0, buffer.Length);

            Assert.AreEqual(buffer.Length - 1, read);
        }

        [TestMethod]
        public void Write()
        {
            var buffer = new byte[stream.Length - 1];
            stream.Write(buffer, 0, buffer.Length);

            Assert.AreEqual(stream.Length - 1, stream.Position);
        }

        [TestMethod]
        public void WriteMoreThanAvailable()
        {
            var buffer = new byte[stream.Length + 1];
            stream.Write(buffer, 0, buffer.Length);

            Assert.AreEqual(stream.Length, stream.Position);
        }

        [TestCleanup]
        public void CleanUp()
        {
            stream.Close();
        }
    }
}
