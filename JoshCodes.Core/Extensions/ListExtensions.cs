using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace JoshCodes.Core.Extensions
{
    public static class ListExtensions
    {
        public static void Each<T>(this System.Collections.Generic.IEnumerable<T> items, Action<T> expr)
        {
            items.Select((item) => { expr(item); return 0; });
        }

        public static List<T> FindAndRemove<T> (this List<T> objs, Expression<Func<T,bool>> expr)
        {
            List<T> objsToKeep = new List<T>(), objsToReturn = new List<T>();
            
            var func = expr.Compile();
            objs.ForEach(t => {
                if((bool) func.DynamicInvoke(t))
                {
                    objsToReturn.Add(t);
                }
                else
                {
                    objsToKeep.Add(t);
                }
            });
            
            objs = objsToKeep;
            return objsToReturn;
        }
        
        public static IDictionary<TINDEX, TOBJ> Index<TINDEX, TOBJ> (this IEnumerable<TOBJ> objs, Func<TOBJ,TINDEX> expr)
        {
            // TODO: If not a list return a datatype that incrementally finds the index
            var dictionary = new Dictionary<TINDEX, TOBJ>();
            foreach(var obj in objs)
            {
                dictionary.Add(expr.Invoke(obj), obj);
            }
            return dictionary;
        }
        
        public static IDictionary<TINDEX, TOBJ> Index<TINDEX, TOBJ> (this IEnumerable<TOBJ> objs, Func<TOBJ,TINDEX> expr, Func<TOBJ, TINDEX, bool> onDuplicateExpr)
        {
            // TODO: If not a list return a datatype that incrementally finds the index
            var dictionary = new Dictionary<TINDEX, TOBJ>();
            foreach(var obj in objs)
            {
                var key = expr.Invoke(obj);
                if(dictionary.ContainsKey(key))
                {
                    if(onDuplicateExpr.Invoke(obj, key))
                    {
                        dictionary[key] = obj;
                    }
                } else
                {
                    dictionary.Add(key, obj);
                }
            }
            return dictionary;
        }

        public delegate TElement CollisionSelectorDelegate<TSource, TKey, TElement>(TSource sourceItem, TKey duplicateKey, TElement currentElement, TElement newElement);

        public static Dictionary<TKey, TElement> ToDictionary<TSource, TKey, TElement>(
            this IEnumerable<TSource> source,
            Func<int, TSource, TKey> keySelector,
            Func<int, TSource, TElement> elementSelector,
            CollisionSelectorDelegate<TSource, TKey, TElement> collisionSelector = null)
        {
            if(collisionSelector == null)
            {
                collisionSelector = (src, key, ele1, ele2) =>
                {
                    throw new ArgumentException("An element with the same key already exists in the dictionary.");
                };
            }
            var dictionary = new Dictionary<TKey, TElement>();
            int index = 0;
            foreach(var obj in source)
            {
                var key = keySelector.Invoke(index, obj);
                var val = elementSelector.Invoke(index, obj);
                if(dictionary.ContainsKey(key))
                {
                    dictionary[key] = collisionSelector.Invoke(obj, key, dictionary[key], val);
                } else
                {
                    dictionary.Add(key, val);
                }
                index++;
            }
            return dictionary;
        }

        public static Dictionary<TKEY, TVALUE> MapToDictionary<TOBJ, TKEY, TVALUE> (
            this IEnumerable<TOBJ> objs,
            Func<TOBJ,TKEY> exprKey,
            Func<TOBJ,TVALUE> exprValue)
        {
            var dictionary = new Dictionary<TKEY, TVALUE>();
            foreach(var obj in objs)
            {
                var key = exprKey.Invoke(obj);
                var val = exprValue.Invoke(obj);
                if(dictionary.ContainsKey(key))
                {
                    key.GetType();
                }
                dictionary.Add(key, val);
            }
            return dictionary;
        }
        
        public static IList<T> MapFromDictionary<T, TKEY, TVALUE> (
            this IDictionary<TKEY, TVALUE> dictionary,
            Func<KeyValuePair<TKEY, TVALUE>, T> expr)
        {
            var list = new List<T>();
            foreach(var kvp in dictionary)
            {
                list.Add(expr.Invoke(kvp));
            }
            return list;
        }

        
        /// <summary>
        /// Flatten converts an "outer" list of "inner" lists to a single inner
        /// list which represents the inner lists concatinated.
        /// </summary>
        /// <param name='objs'>
        /// Outer list
        /// </param>
        /// <param name='expr'>
        /// Extracts the inner list from an element in the outer list
        /// </param>
        /// <typeparam name='TOBJ'>
        /// Type of outer list
        /// </typeparam>
        /// <typeparam name='TRET'>
        /// Type of inner list
        /// </typeparam>
        public static IEnumerable<TRET> Flatten<TOBJ, TRET>(
            this IEnumerable<TOBJ> objs,
            Func<TOBJ, IEnumerable<TRET>> expr)
        {
            IEnumerable<TRET> innerObjs = null;
            foreach(var obj in objs)
            {
                if(innerObjs == null)
                {
                    innerObjs = expr.Invoke(obj);
                } else
                {
                    innerObjs = innerObjs.Concat(expr.Invoke(obj));
                }
            }
            return innerObjs ?? new TRET[0];
        }

        public static IEnumerable<TRET> Flatten<TRET>(
            this IEnumerable<TRET[]> objs)
        {
            return objs.Flatten((obj) => obj);
        }

        public static IEnumerable<TRET> Flatten<TRET>(
            this IEnumerable<IEnumerable<TRET>> objs)
        {
            return objs.Flatten((obj) => obj);
        }
        
        public static Dictionary<TKEY, List<TOBJ>> Group<TKEY, TOBJ>(
            this IEnumerable<TOBJ> objs,
            Func<TOBJ, TKEY> expr)
        {
            var groups = new Dictionary<TKEY, List<TOBJ>>();
            foreach(var obj in objs)
            {
                TKEY key = expr.Invoke(obj);
                if(!groups.ContainsKey(key))
                {
                    groups.Add(key, new List<TOBJ>());
                }
                var list = groups[key];
                list.Add(obj);
            }
            return groups;
        }
        
        public static IEnumerable<T> Unique<T, THASH>(this IEnumerable<T> objs, Func<T, THASH> expr)
        {
            var hash = new HashSet<THASH>();
            var list = new List<T>();
            foreach(var obj in objs)
            {
                var v = expr.Invoke(obj);
                if(!hash.Contains(v))
                {
                    list.Add(obj);
                    hash.Add(v);
                }
            }
            return list.ToArray();
        }
        
        /// <summary>
        /// Operations on IEnumerable like ConvertAll does on IList<T>
        /// </summary>
        /// <returns>
        /// An IEnumerable of a new type.
        /// </returns>
        /// <param name='items'>
        /// Enumeration.
        /// </param>
        /// <typeparam name='TOUT'>
        /// The 1st type parameter.
        /// </typeparam>
        /// <typeparam name='TIN'>
        /// The 2nd type parameter.
        /// </typeparam>
        public static IEnumerable<TOUT> ConvertSequential<TOUT, TIN>(this IEnumerable<TIN> items, Converter<TIN, TOUT> converter)
        {
            // TODO: return an object which converts items sequentially
            foreach(var item in items)
            {
                yield return converter.Invoke(item);
            }
        }

        public static string ToJSArray(this IEnumerable<string> objs)
        {
            var sb = new StringBuilder();

            sb.Append("[");
            sb.Append(string.Join(",", objs.ToArray()));
            sb.Append("]");

            return sb.ToString();
        }
    }
}

