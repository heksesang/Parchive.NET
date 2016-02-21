using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Linq
{
    public static class IImmutableCollectionExtensions
    {
        #region Extensions for IImmutableList<T>
        public static int IndexOf<TSource, TCompareKey>(this IImmutableList<TSource> source, TSource item, int index, int count, Func<TSource, TCompareKey> compareKeySelector)
        {
            return source.IndexOf(item, index, count, AnonymousComparer.Create(compareKeySelector));
        }

        public static int LastIndexOf<TSource, TCompareKey>(this IImmutableList<TSource> source, TSource item, int index, int count, Func<TSource, TCompareKey> compareKeySelector)
        {
            return source.LastIndexOf(item, index, count, AnonymousComparer.Create(compareKeySelector));
        }

        public static IImmutableList<TSource> Remove<TSource, TCompareKey>(this IImmutableList<TSource> source, TSource value, Func<TSource, TCompareKey> compareKeySelector)
        {
            return source.Remove(value, AnonymousComparer.Create(compareKeySelector));
        }

        public static IImmutableList<TSource> RemoveRange<TSource, TCompareKey>(this IImmutableList<TSource> source, IEnumerable<TSource> items, Func<TSource, TCompareKey> compareKeySelector)
        {
            return source.RemoveRange(items, AnonymousComparer.Create(compareKeySelector));
        }

        public static IImmutableList<TSource> Replace<TSource, TCompareKey>(this IImmutableList<TSource> source, TSource oldValue, TSource newValue, Func<TSource, TCompareKey> compareKeySelector)
        {
            return source.Replace(oldValue, newValue, AnonymousComparer.Create(compareKeySelector));
        }
        #endregion
    }
}
