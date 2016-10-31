using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZaloCommunityDev.Shared
{
    public static class Extensions
    {
        public static T ElementAtOrDefault<T>(this IEnumerable<T> v, int at, T @default  =default(T))
        {
            if (v.Count() > at)
                return v.ElementAt(at);

            return @default;

        }
    }
}
