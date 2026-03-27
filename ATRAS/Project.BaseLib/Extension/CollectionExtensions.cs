using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.BaseLib.Extension
{
  public static class CollectionExtensions
  {
    public static bool IsNullOrEmpty(this IList list)
    {
      return list == null || list.Count == 0;
    }
    public static IEnumerable<TAccumulate> SelectAggregate<TSource, TAccumulate>(
      this IEnumerable<TSource> source, 
      TAccumulate seed,
      Func<TAccumulate, TSource, int, TAccumulate> func)
    {
      int index = -1;
      TAccumulate previous = seed;
      foreach(var item in source)
      {
        checked { index++; }
        TAccumulate result = func(previous, item, index);
        previous = result;
        yield return result;
      }
    }

    public static IEnumerable<TResult> Repeat<TResult>(Func<TResult> newElement, int count)
    {
      if (count < 0) throw new ArgumentOutOfRangeException("count");
      return RepeatIterator<TResult>(newElement, count);
    }
    static IEnumerable<TResult> RepeatIterator<TResult>(Func<TResult> newElement, int count)
    {
      for (int i = 0; i < count; i++) yield return newElement();
    }

    public static void SortAndRemoveDuplicates<T>(this List<T> list)
    {
      list.Sort();
      RemoveDuplicatesFromSortedList<T>(list);
    }
    public static void RemoveDuplicatesFromSortedList<T>(this IList<T> list)
    {
      for (int i = list.Count - 1; i >= 1; --i)
        if (list[i].Equals(list[i - 1]))
          list.RemoveAt(i);
    }

    public static int BinarySearch<T>(this IList<T> list, T value)
    {
      return BinarySearch(list, value, Comparer<T>.Default);
    }

    public static int BinarySearch<T>(this IList<T> list, T value, IComparer<T> comparer)
    {
      if (list == null)
        throw new ArgumentNullException("list");
      else if (comparer == null)
        throw new ArgumentNullException("comparer");

      int lower = 0;
      int upper = list.Count - 1;
      int middle = 0;
      int comparisonResult;
      while(lower <= upper)
      {
        middle = (lower + upper) / 2;
        comparisonResult = comparer.Compare(value, list[middle]);
        if (comparisonResult == 0)
          return middle;
        else if (comparisonResult < 0)
          upper = middle - 1;
        else
          lower = middle + 1;
      }
      return ~lower;
    }

    public static void RemoveLast<T>(this IList<T> list)
    {
      list.RemoveAt(list.Count - 1);
    }
    public static void RemoveFirst<T>(this IList<T> list)
    {
      list.RemoveAt(0);
    }

    public static bool IsIdentical(this IList list1, IList list2)
    {
      return IsIdentical<object, object>(list1, list2, null);
    }

    public static bool IsIdentical<TSource, TKey>(this IList list1, IList list2, Func<TSource, TKey> selector = null)
    {
      if (list1 == list2)
        return true;
      else if (list1 == null || list2 == null)
        return false;
      else if (list1.Count != list2.Count)
        return false;
      else
      {
        for(int i = 0; i < list1.Count; ++i)
        {
          if (list1[i] == null || list2[i] == null)
            return false;

          if(list1[i] is IList)
          {
            if (!(list1[i] as IList).IsIdentical<TSource, TKey>(list2[i] as IList))
              return false;
          }
          else
          {
            if(selector == null)
            {
              if (!list1[i].Equals(list2[i]))
                return false;
            }
            else
            {
              if (!selector((TSource)list1[i]).Equals(selector((TSource)list2[i])))
                return false;
            }
          }
        }
      }
      return true;
    }

    public static string ToStringItems<T>(this T[] collection, int MaxItems = Int32.MaxValue)
    {
      StringBuilder builder = new StringBuilder();
      builder.AppendFormat("{0} ", collection.Length).Append(collection.Length == 1 ? "Item [" : "Items" + " [");

      for(int index = 0; index < collection.Length && index < MaxItems; index++)
      {
        var item = collection[index];
        if (item == null)
          builder.Append("null ,");
        else
          builder.AppendFormat("{0},", item.ToString());
      }

      // Remove the final delimiter
      builder.Remove(builder.Length - 1, 1);
      builder.Append(" ]");
      return builder.ToString();
    }

    public static string ToStringItems<T>(this IEnumerable<T> collection, int MaxItems = Int32.MaxValue)
    {
      StringBuilder builder = new StringBuilder();
      builder.AppendFormat("{0} ", collection.Count()).Append(collection.Count() == 1 ? "Item [" : "Items" + " [");
      for (int index = 0; index < collection.Count() && index < MaxItems; index++)
      {
        var item = collection.ElementAt(index);
        if (item == null)
          builder.Append("null ,");
        else
          builder.AppendFormat("{0},", item.ToString());
      }

      // Remove the final delimiter
      builder.Remove(builder.Length - 1, 1);
      builder.Append(" ]");
      return builder.ToString();
    }

    public static string ToStringItems<T, V>(this IEnumerable<T> collection, int MaxItems = Int32.MaxValue) where T : IEnumerable<V>
    {
        StringBuilder builder = new StringBuilder();
        builder.AppendFormat("{0} ", collection.Count()).Append(collection.Count() == 1 ? "Item [" : "Items" + " [");

        for (int index = 0; index < collection.Count() && index < MaxItems; index++)
        {
            var item = collection.ElementAt(index);
            if (item == null)
                builder.Append("null ,");
            else
                builder.AppendFormat("{0},", item.ToStringItems());
        }

        // Remove the final delimiter
        builder.Remove(builder.Length - 1, 1);
        builder.Append(" ]");
        return builder.ToString();
    }

    public static string ToStringItems<Key, Value>(this Dictionary<Key, Value> collection, int MaxItems = Int32.MaxValue)
    {
      StringBuilder builder = new StringBuilder();
      builder.AppendFormat("{0} ", collection.Count()).Append(collection.Count() == 1 ? "Item [" : "items" + " [");
      foreach(KeyValuePair<Key, Value> pair in collection)
      {
        builder.Append("<Key=").Append(pair.Key).Append(",Value=").Append(pair.Value).Append(">,");
      }
      string result = builder.ToString();
      result = result.TrimEnd(',');
      result += " ]";
      return result;
    }

    public static string ToStringItems<Key, Value, T>(this Dictionary<Key, Value> collection, int MaxItems = Int32.MaxValue)
    {
        StringBuilder builder = new StringBuilder();
        builder.AppendFormat("{0} ", collection.Count()).Append(collection.Count() == 1 ? "Item [" : "Items" + " [");
        foreach (KeyValuePair<Key, Value> pair in collection)
        {
            if (pair.Value is IList<T>)
            {
                IList<T> value = (IList<T>)pair.Value;
                builder.Append("<Key=").Append(pair.Key).Append(",Value=").Append(value.ToStringItems()).Append(">,");
            }
        }
        string result = builder.ToString();

        result = result.TrimEnd(',');
        result += " ]";
        return result;
    }

    public static Double StdDev(this IEnumerable<double> values)
    {
      Double mean = 0.0;
      Double sum = 0.0;
      Double stdDev = 0.0;
      int n = 0;
      foreach(double val in values)
      {
        n++;
        double delta = val - mean;
        mean += delta / n;
        sum += delta * (val - mean);
      }
      if (1 < n)
        stdDev = Math.Sqrt(sum / (n - 1));

      return stdDev;
    }
  }
}
