using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;

namespace Project.BaseLib.Extension
{
  public static class PathExtensions
  {
    private static Dictionary<string, string> invalidCharToStringMap = new Dictionary<string, string>()
    {
      {"<>", "not equal" },
      {"<=", "less than or equals" },
      {">=", "greater than or equals" },
      {"<", "less than" },
      {">", "greater than" }
    };

    public static string ReplaceInvalidCharacters(this string str)
    {
      string tmp = invalidCharToStringMap.Aggregate(str, (current, c) =>
      {
        return current.Replace(c.Key, c.Value);
      });

      return Path.GetInvalidFileNameChars().Aggregate(tmp, (current, c) =>
      {
        return current.Replace(c.ToString(), "");
      });
    }
  }
}
