using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PolyPaint.Utils
{
    public static class CollectionUtils
    {
        public static void AddAll<T>(this ICollection<T> first, ICollection<T> second, bool ignoreDuplicates = false)
        {
            if (second == null)
                return;

            foreach (T item in second)
            {
                if (!ignoreDuplicates || !first.Contains(item))
                {
                    first.Add(item);
                }            
            }
        }

        public static void RemoveAll<T>(this ICollection<T> first, ICollection<T> second)
        {
            if (second == null)
                return;

            foreach (T item in second)
            {
                first.Remove(item);
            }
        }

        public static void RemoveAll<T>(this ICollection<T> collection, Func<T,bool> predicate)
        {
            var toRemove = new List<T>();
            foreach (T item in collection)
            {
                if (predicate(item))
                {
                    toRemove.Add(item);
                }
            }
            collection.RemoveAll(toRemove);
        }

        public static void Update<T>(this ICollection<T> first, ICollection<T> second)
        {
            if (second == null)
            {
                first.Clear();
            }
            else
            {
                try
                {
                    first.RemoveAll(item => !second.Contains(item));
                    first.AddAll(second, ignoreDuplicates: true);
                }
                catch(Exception)
                {}
            }
        }
    }
}
