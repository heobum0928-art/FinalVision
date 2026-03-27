using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.BaseLib.Extension
{
    public static class MathExtensions
    {
        public static bool Between<T>(this T actual, T lower, T upper) where T : IComparable<T>
        {
            return actual.CompareTo(lower) >= 0 && actual.CompareTo(upper) <= 0;
        }

        public static IEnumerable<double> Arange(double start, int count)
        {
            return Enumerable.Range((int)start, count).Select(v => (double)v);
        }

        public static double limit(this double actual, double lower, double upper)
        {
            var t = Math.Max(lower, actual);
            
            return Math.Min(t, upper);
        }

        public static int limit(this int actual, int lower, int upper)
        {
            var t = Math.Max(lower, actual);

            return Math.Min(t, upper);
        }

        public static byte limit(this byte actual, byte lower, byte upper)
        {
            var t = Math.Max(lower, actual);

            return Math.Min(t, upper);
        }
    }
}
