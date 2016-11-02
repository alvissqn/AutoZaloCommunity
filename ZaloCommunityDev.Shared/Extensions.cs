using System;
using System.Collections.Generic;
using System.Linq;
using ZaloCommunityDev.Shared.Structures;

namespace ZaloCommunityDev.Shared
{
    public static class Extensions
    {
        public static T ElementAtOrDefault<T>(this IEnumerable<T> v, int at, T @default = default(T))
        {
            if (v.Count() > at)
                return v.ElementAt(at);

            return @default;
        }

        public static bool IsValidProfile(this Filter filter, ProfileMessage profile, out string reason)
        {
            reason = null;
            if (string.IsNullOrWhiteSpace(filter.FilterAgeRange))
            {
                var ages = filter.FilterAgeRange.Split(";-=_ ".ToArray());

                int from = int.Parse(ages[0]);
                int to = int.Parse(ages[1]);

                DateTime date;
                if(DateTime.TryParse(profile.BirthdayText, out date))
                {
                    var profileAge = DateTime.Now.Year - date.Year;

                    if(from <=profileAge && profileAge <= to)
                    {

                    }
                    else
                    {
                        reason = $"Độ tuổi {profileAge} không trong khoảng [{from}-{to}]";

                        return false;
                    }
                }
            }

            return true;
        }
    }
}