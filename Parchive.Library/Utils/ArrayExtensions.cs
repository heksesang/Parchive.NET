using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parchive.Library
{
    public static class ArrayExtensions
    {
        /// <summary>
        /// Sets all values in the array.
        /// </summary>
        /// <typeparam name="T">The array type.</typeparam>
        /// <param name="array">The array.</param>
        /// <param name="value">The value to set.</param>
        /// <returns>The array.</returns>
        public static T[] SetAllValues<T>(this T[] array, T value) where T : struct
        {
            for (int i = 0; i < array.Length; i++)
                array[i] = value;

            return array;
        }
    }
}
