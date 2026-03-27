using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Xml.Serialization;

namespace Project.BaseLib.Extension
{
  public class DictionaryEntry<TKey, TValue>
  {
    [XmlAttribute]
    public TKey Key { get; set; }

    public TValue Value { get; set; }

    public DictionaryEntry() { }
    public DictionaryEntry(KeyValuePair<TKey, TValue> pair) : this(pair.Key, pair.Value) { }

    public DictionaryEntry(TKey key, TValue value)
    {
      this.Key = key;
      this.Value = value;
    }
  }
  public static class DictionaryExtensions
  {

    public static Dictionary<TKey, TValue> ToDictionary<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, TValue>> collection)
    {
      Dictionary<TKey, TValue> dictionary = new Dictionary<TKey, TValue>();

      foreach (KeyValuePair<TKey, TValue> pair in collection)
        dictionary[pair.Key] = pair.Value;

      return dictionary;
    }

    public static bool IsIdentical<TKey, TValue>(this IDictionary<TKey, TValue> dictionary1, IDictionary<TKey, TValue> dictionary2)
    {
      if (dictionary1 == dictionary2)
        return true;

      if (dictionary1 == null || dictionary2 == null)
        return false;

      if (dictionary1.Count != dictionary2.Count)
        return false;

      foreach(KeyValuePair<TKey, TValue> pair in dictionary1)
      {
        if (!dictionary2.ContainsKey(pair.Key))
          return false;

        if(pair.Value != null)
        {
          if (pair.Value is IList)
          {
            if (!(pair.Value as IList).IsIdentical(dictionary2[pair.Key] as IList))
              return false;
          }
          else if (!pair.Value.Equals(dictionary2[pair.Key]))
            return false;
        }
      }
      return true;
    }

    public static TValue? GetNullableValue<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key)
      where TValue : struct
    {
      TValue val;

      if (dictionary.TryGetValue(key, out val))
        return val;

      return null;
    }

    public static TValue GetDefaultValue<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key)
    {
      return GetDefaultValue<TKey, TValue>(dictionary, key, default(TValue));
    }
    
    public static TValue GetDefaultValue<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue defaultValue)
    {
      TValue value;

      if (dictionary.TryGetValue(key, out value))
        return value;

      return defaultValue;
    }
  }
}
