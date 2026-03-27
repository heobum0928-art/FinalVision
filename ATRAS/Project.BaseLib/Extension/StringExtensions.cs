using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.BaseLib.Extension
{
  public static class StringExtensions
  {
    //public static string ToStringItems<T>(this IEnumerable<T> collection, int MaxItems = int.MaxValue)
    //{
    //    StringBuilder stringBuilder = new StringBuilder();
    //    stringBuilder.AppendFormat("{0} ", collection.Count()).Append((collection.Count() == 1) ? "Item [" : "Items [");
    //    for (int i = 0; i < collection.Count() && i < MaxItems; i++)
    //    {
    //        T val = collection.ElementAt(i);
    //        if (val == null)
    //        {
    //            stringBuilder.Append("null ,");
    //        }
    //        else
    //        {
    //            stringBuilder.AppendFormat("{0},", val.ToString());
    //        }
    //    }

    //    stringBuilder.Remove(stringBuilder.Length - 1, 1);
    //    stringBuilder.Append(" ]");
    //    return stringBuilder.ToString();
    //}
  }
}
