using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JoshCodes.Core.Extensions
{
    public static class IEnumerableExtensions
    {
        public static void ForEach<T>(this IEnumerable<T> items, Action<T> action)
        {
            foreach (var item in items)
            {
                action(item);
            }
        }
        
        //public static IDictionary<TKey, TValue> ToDictionary<TKey, TValue, TItem>(
        //    this IEnumerable<TItem> items, Func<TItem, TKey> keySelector, Func<TItem, TValue> valueSelector)
        //{
        //    var dictionary = new Dictionary<TKey, TValue>();
        //    foreach (var item in items)
        //    {
        //        dictionary.Add(keySelector.Invoke(item), valueSelector.Invoke(item));
        //    }
        //    return dictionary;
        //}

        public static IDictionary<TKey, TValue> ToDictionary<TKey, TValue>(
            this IEnumerable<KeyValuePair<TKey, TValue>> items)
        {
            var dictionary = new Dictionary<TKey, TValue>();
            foreach (var item in items)
            {
                dictionary.Add(item.Key, item.Value);
            }
            return dictionary;
        }

        public static IEnumerable<T> ToEnumerable<T>(this T item)
        {
            yield return item;
        }

        public static T SelectRandom<T>(this IEnumerable<T> items, int total, Random rand = null)
        {
            if (rand == null)
            {
                rand = new Random();
            }
            var totalD = (double)total;
            var arrayItems = new T[total];
            var arrayItemsIndex = 0;
            foreach (var item in items)
            {
                if (rand.NextDouble() < (1.0 / totalD))
                {
                    return item;
                }
                totalD -= 1.0;
                arrayItems[arrayItemsIndex] = item;
                arrayItemsIndex++;
            }
            if (arrayItemsIndex == 0)
            {
                return default(T);
            }
            var selectedIndex = (int)(arrayItemsIndex * rand.NextDouble());
            return arrayItems[selectedIndex];
        }

        public static T SelectRandom<T>(this IEnumerable<T> items, Random rand = null)
        {
            return items.SelectRandom(items.Count(), rand);
        }

        public static TItem MinOn<TItem, TValue>(this IEnumerable<TItem> items, Func<TItem, TValue> selector)
            where TValue : IComparable
        {
            bool firstValueSet = false;
            var minValue = default(TValue);
            var minItem = default(TItem);
            foreach (var item in items)
            {
                var candidateValue = selector.Invoke(item);
                if (!firstValueSet)
                {
                    minItem = item;
                    minValue = candidateValue;
                    firstValueSet = true;
                    continue;
                }
                if (candidateValue.CompareTo(minValue) < 0)
                {
                    minItem = item;
                    minValue = candidateValue;
                }
            }
            return minItem;
        }

        public static IEnumerable<T> SubsetRandom<T>(this IEnumerable<T> items, int min, int max)
        {
            var rand = new Random();
            var remainingItems = new List<T>(items);

            var total = new Random().Between(min, max);
            int count = 0;
            while (count < total)
            {
                if(remainingItems.Count == 0)
                {
                    if(count < min)
                    {
                        throw new ArgumentException("min", "Minimum is greater than total number of items from which to subselect");
                    }
                    yield break;
                }
                var selectedItem = remainingItems.SelectRandom();
                remainingItems.Remove(selectedItem);
                yield return selectedItem;
                count++;
            }
        }
        
        public delegate TOut AggregateWithSelectorDelegate<TOut, TIn, TWith>(TIn item, TWith seed, out TWith mutatedSeed);
        public static IEnumerable<TOut> AggregateWith<TOut, TIn, TWith>(this IEnumerable<TIn> items, TWith seed,
            AggregateWithSelectorDelegate<TOut, TIn, TWith> selector)
        {
            foreach(var item in items)
            {
                yield return selector.Invoke(item, seed, out seed);
            }
        }

        public static IEnumerable<TResult> SelectWithIndex<TSource, TResult>(
            this IEnumerable<TSource> source,
            Func<int, TSource, TResult> selector)
        {
            int index = 0;
            return source.Select<TSource, TResult>((item) => {
                var selected = selector.Invoke(index, item);
                index++;
                return selected;
            });
        }

        public static IEnumerable<TResult> SelectManyWithIndex<TSource, TResult>(
            this IEnumerable<TSource> source,
            Func<int, TSource, IEnumerable<TResult>> selector)
        {
            int index = 0;
            return source.SelectMany<TSource, TResult>((item) => {
                var selected = selector.Invoke(index, item);
                index++;
                return selected;
            });
        }

        public static async Task<int> SumAsync<T>(this IEnumerable<T> items, Func<T, Task<int>> selector)
        {
            var tasks = items.Select(item => selector.Invoke(item));
            var values = await Task.WhenAll(tasks);
            return values.Sum(value => value);
        }

        public static IEnumerable<T>[] Distribute<T>(this IEnumerable<T> items, double [] weights)
        {
            if (weights.Length == 0)
                return new IEnumerable<T>[] { (IEnumerable<T>)(new List<T>()) };

            var totalWeight = weights.Sum(weight => weight);
            var normalizedWeights = weights.Select(weight => weight / totalWeight);
            var rand = new Random();
            var results = weights.Select((weight) => new List<T>()).ToArray();
            foreach(var item in items)
            {
                var total = 0.0;
                var cutOff = rand.NextDouble();
                var index = 0;
                foreach (var normalizedWeight in normalizedWeights)
                {
                    total += normalizedWeight;
                    if (total > cutOff)
                    {
                        results[index].Add(item);
                        break;
                    }
                    index++;
                }
                // throw new InvalidOperationException("Failed to normalize rates");
            }
            return results;
        }

        public static IEnumerable<T[]> TakeMany<T>(this IEnumerable<T> items, int count)
        {
            while(true)
            {
                var subset = items.Take(count).ToArray();

                if (subset.Length != 0)
                    yield return subset;

                if (subset.Length != count)
                    yield break;

                items = items.Skip(count);
            }
        }

        /// <summary>
        /// Combines neighboring elements into a new composition list.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="items">Set over which to operate</param>
        /// <param name="folder">Method for folding items</param>
        /// <returns>Given items [1,2,3,4,5] returns [a,b,c,d] such that
        /// a = folder(1,2), b = folder(2,3), c = folder(3,4), d = folder(4,5)</returns>
        public static IEnumerable<TResult> Fold<TSource, TResult>(this IEnumerable<TSource> items,
            Func<TSource, TSource, TResult> folder)
        {
            bool hasRunOnce = false;
            var last = default(TSource);
            foreach(var item in items)
            {
                if (hasRunOnce)
                    yield return folder.Invoke(last, item);
                hasRunOnce = true;
                last = item;
            }
        }

        /// <summary>
        /// Normalize takes a list, splits it into two lists, then combines those list into a dictionary
        /// where items from the first list are unique  
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="items"></param>
        /// <param name="keySelector"></param>
        /// <param name="valueSelector"></param>
        /// <returns></returns>
        public static IDictionary<TKey, IEnumerable<TValue>> Normalize<TKey, TValue, TSource>(this IEnumerable<TSource> items,
            Func<TSource, TKey> keySelector, Func<TSource, TValue> valueSelector)
            where TKey : IComparable
        {
            var values = items.Select(valueSelector);
            var keys = items.Select(keySelector).Unique(item => item);
            var dictionary = keys
                .ToDictionary(key => key, key => values.WhereWith(keys, k => k.CompareTo(key) == 0));
            return dictionary;
        }

        public static IEnumerable<T1> WhereWith<T1>(this IEnumerable<T1> items,
            IEnumerable<bool> withs)
        {
            var itemEnumerator = items.GetEnumerator();
            var withEnumerator = withs.GetEnumerator();
            while(itemEnumerator.MoveNext())
            {
                // Loop over the withs if the end is reached
                if (!withEnumerator.MoveNext())
                {
                    withEnumerator = withs.GetEnumerator();
                    if (!withEnumerator.MoveNext())
                        throw new ArgumentException("withs is empty IEnumerable");
                }
                if (withEnumerator.Current)
                    yield return itemEnumerator.Current;
            }
        }

        public static IEnumerable<T1> WhereWith<T1, T2>(this IEnumerable<T1> items,
            IEnumerable<T2> withs, Func<T2, bool> predicate)
        {
            var booleanWiths = withs.Select(with => predicate(with));
            return items.WhereWith(booleanWiths);
        }

        /// <summary>
        /// Index 0 is even so starts with even (comp sci, not math)
        /// </summary>
        public static IEnumerable<KeyValuePair<TEven, TOdd>> SelectEvenOdd<TSelect, TEven, TOdd>(
            this IEnumerable<TSelect> items, Func<TSelect, TEven> evenSelect, Func<TSelect, TOdd> oddSelect)
        {
            bool even = true;
            TEven evenValue = default(TEven);
            foreach (var item in items)
            {
                if (even)
                    evenValue = evenSelect.Invoke(item);
                else
                    yield return new KeyValuePair<TEven, TOdd>(evenValue, oddSelect.Invoke(item));
                even = !even;
            }
        }
    }
}
