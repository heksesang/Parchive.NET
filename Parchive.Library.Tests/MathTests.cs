using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Parchive.Library.Math.Galois;
using Parchive.Library.Math;

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

        [TestMethod]
        public void Restore()
        {
            //var values = new Tuple<int, int>[]
            //{
            //    Tuple.Create(5, 2),
            //    Tuple.Create(6, 4)
            //};
            //var exp = new int[] { 1, 2 };

            var values = new List<Tuple<int, int>>();
            var exp = new List<int>();

            for (int i = 1, e = 1; i <= 4096; i++)
            {
                var constant = table.Pow(2, e);
                values.Add(Tuple.Create(i, constant));

                exp.Add(i);

                do
                {
                    e++;
                } while (!(e % 3 != 0 && e % 5 != 0 && e % 17 != 0 && e % 257 != 0));
            }

            var parity = new List<Tuple<int, int, int>>();
            
            for (int i = 0; i < exp.Count; i++)
            {
                var e = exp[i];
                var p = Recovery.generateErrorCode(values, e);
                parity.Add(Tuple.Create(values[i].Item2, e, p));
            }

            var source = Recovery.restoreData(parity);
            
            for (int i = 0; i < source.Length; i++)
            {
                Assert.AreEqual(values[i].Item1, source[i]);
            }
        }
    }
}
