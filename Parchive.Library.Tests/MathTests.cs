using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Parchive.Library.RecoveryMath.Galois;
using Parchive.Library.RecoveryMath;

namespace Parchive.Library.Tests
{
    [TestClass]
    public class MathTests
    {
        Table table = new Table(16, 0x1100B);

        [TestMethod]
        public void GFAdd()
        {
            Assert.AreEqual(127, table.Add(15, 112));
        }

        [TestMethod]
        public void GFSubtract()
        {
            Assert.AreEqual(127, table.Sub(15, 112));
        }

        [TestMethod]
        public void GFMultiply()
        {
            Assert.AreEqual(720, table.Mul(15, 112));
        }

        [TestMethod]
        public void GFDivide()
        {
            Assert.AreEqual(17076, table.Div(15, 112));
        }

        [TestMethod]
        public void GFPower()
        {
            Assert.AreEqual(85, table.Pow(15, 2));
        }
    }
}
