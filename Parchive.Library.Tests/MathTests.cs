using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Parchive.Library.Math;

namespace Parchive.Library.Tests
{
    [TestClass]
    public class MathTests
    {
        [TestMethod]
        public void GFAdd()
        {
            GF16 a = 256;
            GF16 b = 255;

            var p = a + b;
            Assert.AreEqual(511, p.Value);
        }

        [TestMethod]
        public void GFSub()
        {
            GF16 a = 256;
            GF16 b = 128;

            var p = a - b;
            Assert.AreEqual(384, p.Value);
        }

        [TestMethod]
        public void GFMult()
        {
            GF16 a = 256;
            GF16 b = 2;

            var p = a * b;
            Assert.AreEqual(512, p);
        }

        [TestMethod]
        public void GFDiv()
        {
            GF16 a = 512;
            GF16 b = 2;

            var p = a / b;
            Assert.AreEqual(256, p);
        }
    }
}
