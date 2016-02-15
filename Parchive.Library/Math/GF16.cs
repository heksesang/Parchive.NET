using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parchive.Library.Math
{
    /// <summary>
    /// Implementation of 16-bit Galois Field math.
    /// </summary>
    public class GF16
    {
        #region Fields
        // Generator
        private const uint _Generator = 0x0001100B;

        // The upper limit of the table
        private const ushort _Limit = ushort.MaxValue;
        #endregion

        #region Static Fields
        // Log and antilog tables
        static private ushort[] _Log = new UInt16[_Limit + 1];
        static private ushort[] _AntiLog = new UInt16[_Limit + 1];
        #endregion

        #region Static Initialization
        /// <summary>
        /// Static ctor.
        /// </summary>
        static GF16()
        {
            uint b = 1;

            for (uint l = 0; l < _Limit; l++)
            {
                _Log[b] = (ushort)l;
                _AntiLog[l] = (ushort)b;

                b <<= 1;
                if ((b & (_Limit + 1)) != 0) b ^= _Generator;
            }

            _Log[0] = _Limit;
            _AntiLog[_Limit] = 0;
        }
        #endregion

        #region Constructors
        /// <summary>
        /// Default ctor.
        /// </summary>
        /// <param name="Value">A 16-bit value. Default value: 0.</param>
        public GF16(ushort value = 0)
        {
            this.Value = value;
        }
        #endregion

        #region Properties
        /// <summary>
        /// The 16-bit value of this GF object.
        /// </summary>
        public ushort Value { get; set; }
        #endregion

        #region Operators
        /// <summary>
        /// Implicit conversion from UInt16.
        /// </summary>
        /// <param name="d"></param>
        public static implicit operator GF16(ushort value)
        {
            return new GF16(value);
        }

        /// <summary>
        /// Implicit conversion to UInt16.
        /// </summary>
        /// <param name="d"></param>
        public static implicit operator ushort(GF16 gf)
        {
            return gf.Value;
        }

        /// <summary>
        /// Addition operator
        /// </summary>
        public static GF16 operator+(GF16 left, GF16 right)
        {
            return (ushort)(left.Value ^ right.Value);
        }

        /// <summary>
        /// Subtraction operator
        /// </summary>
        public static GF16 operator-(GF16 left, GF16 right)
        {
            return (ushort)(left.Value ^ right.Value);
        }

        public static GF16 operator*(GF16 left, GF16 right)
        {
            if (left.Value == 0 || right.Value == 0) return 0;

            uint sum = (uint)_Log[left.Value] + _Log[right.Value];
            if (sum >= _Limit)
            {
                return _AntiLog[sum - _Limit];
            }
            else
            {
                return _AntiLog[sum];
            }
        }

        public static GF16 operator/(GF16 left, GF16 right)
        {
            if (left.Value == 0) return 0;
            
            if (right.Value == 0) { throw new DivideByZeroException(); }

            int sum = _Log[left.Value] - _Log[right.Value];
            if (sum < 0)
            {
                return _AntiLog[sum + _Limit];
            }
            else
            {
                return _AntiLog[sum];
            }
        }

        public static GF16 operator^(GF16 left, GF16 right)
        {
            if (right.Value == 0) return 1;
            if (left.Value == 0) return 0;

            uint sum = (uint)_Log[left.Value] * (uint)right.Value;

            sum = (sum >> 16) + (sum & _Limit);
            if (sum >= _Limit)
            {
                return _AntiLog[sum - _Limit];
            }
            else
            {
                return _AntiLog[sum];
            }
        }
        #endregion
    }
}
