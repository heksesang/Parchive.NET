using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parchive.Library
{
    public static class ArrayExtensions
    {
        public static T[] SetAllValues<T>(this T[] array, T value) where T : struct
        {
            for (int i = 0; i < array.Length; i++)
                array[i] = value;

            return array;
        }
    }
}
