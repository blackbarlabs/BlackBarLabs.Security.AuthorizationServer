using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JoshCodes.Core.Extensions
{
    public static class DictionaryExtensions
    {
        public static string ToDelimitedString<TKey, TValue>(
            this IDictionary<TKey, TValue> dictionary, string keyValueDelimeter, string sequenceDelimeter)
        {
            var stringBuilder = new StringBuilder();
            dictionary.ForEach(x => stringBuilder.AppendFormat(
                "{0}{1}{2}{3}",
                x.Key.ToString(),
                keyValueDelimeter,
                x.Value.ToString(),
                sequenceDelimeter
            ));
            return stringBuilder.ToString(0, stringBuilder.Length - sequenceDelimeter.Length);
        }

        public static TValue GetOrCreate<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, Func<TKey, TValue> createExpr)
        {
            if(dictionary.ContainsKey(key))
            {
                return dictionary[key];
            }
            var newValue = createExpr.Invoke(key);
            dictionary.Add(key, newValue);
            return newValue;
        }

        public static void CreateOrModify<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, Func<TKey, TValue> createExpr, Action<TValue> modifyExpr)
        {
            if(dictionary.ContainsKey(key))
            {
                modifyExpr.Invoke(dictionary[key]);
            } else
            {
                var newValue = createExpr.Invoke(key);
                dictionary.Add(key, newValue);
            }
        }

        public static void CreateOrReplace<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue value)
        {
            if(dictionary.ContainsKey(key))
            {
                dictionary[key] = value;
            } else
            {
                dictionary.Add(key, value);
            }
        }

        public static IEnumerable<TValue> GetValuesList<TKey, TValue>(this IDictionary<TKey, TValue> dictionary)
        {
            foreach(var kvp in dictionary) { yield return kvp.Value; }
        }

        public static IEnumerable<TKey> GetKeysListWithValuesMask<TKey>(this IDictionary<TKey, bool> dictionary)
        {
            foreach(var kvp in dictionary)
            {
                if(kvp.Value)
                {
                    yield return kvp.Key;
                }
            }
        }

        public static void Add<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, IDictionary<TKey, TValue> items)
        {
            foreach(var itemKVP in items)
            {
                dictionary.Add(itemKVP.Key, itemKVP.Value);
            }
        }

        public delegate TValue ResolveUnionConflictDelegate<TKey, TValue>(TKey key, TValue value1, TValue value2);

        public static void Merge<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, IDictionary<TKey, TValue> items)
        {
            dictionary.Merge(items, (key, value1, value2) => value1);
        }

        public static void Merge<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, IDictionary<TKey, TValue> items, ResolveUnionConflictDelegate<TKey, TValue> resolve)
        {
            foreach(var itemKVP in items)
            {
                var key = itemKVP.Key;
                if(dictionary.ContainsKey(key))
                {
                    dictionary[key] = resolve.Invoke(key, dictionary[key], itemKVP.Value);
                } else
                {
                    dictionary.Add(key, itemKVP.Value);
                }
            }
        }


        public static IDictionary<TIndex, TElement> Convert<TSource, TKey, TElement, TIndex>(
            this IDictionary<TKey, TSource> source,
            Func<TKey, TSource, TIndex> indexSelector,
            Func<TKey, TSource, TElement> elementSelector)
        {
            var lookup = new Dictionary<TIndex, TElement>();
            foreach(var kvp in source)
            {
                var index = indexSelector.Invoke(kvp.Key, kvp.Value);
                var val = elementSelector.Invoke(kvp.Key, kvp.Value);
                lookup.Add(index, val);
            }
            return lookup;
        }

        public static IDictionary<TKey, TValue> Convert<TKey, TValue>(
            this System.Collections.IDictionary source,
            Func<object, object, TKey> keySelector,
            Func<object, object, TValue> valueSelector)
        {
            var lookup = new Dictionary<TKey, TValue>();
            
            foreach (var key in source.Keys)
            {
                var value = source[key];
                var index = keySelector.Invoke(key, value);
                var val = valueSelector.Invoke(key, value);
                lookup.Add(index, val);
            }
            return lookup;
        }

        public static string KeysAsSeparatedString<TKey, TValue>(
            this System.Collections.Generic.IDictionary<TKey, TValue> source,
            string separator,
            Func<TKey, string> keyStringSelector)
        {
            var stringKeys = source.Keys.Select((k) => keyStringSelector.Invoke(k));
            var keysAsSeparatedString = String.Join(separator, stringKeys);
            return keysAsSeparatedString;
        }

        public static string ConvertToJSObject(this IDictionary<string,string> jsFields)
        {
            var sb = new StringBuilder();

            sb.Append("{");
            foreach(var jsField in jsFields)
            {
                var isArrayOrObj = (jsField.Value != null && jsField.Value.Length > 0 && 
                    (
                        (jsField.Value[0] == '[' && jsField.Value[jsField.Value.Length-1] == ']') ||
                        (jsField.Value[0] == '{' && jsField.Value[jsField.Value.Length-1] == '}')
                    ));
                var val = (isArrayOrObj) ? jsField.Value : string.Format(@"""{0}""", jsField.Value);

                sb.Append(string.Format(@"""{0}"":{1},", jsField.Key, val));
            }

            sb.Remove(sb.Length-1, 1);
            sb.Append("}");

            return sb.ToString();
        }
    }
}

