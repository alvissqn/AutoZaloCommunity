using System;
using System.Collections.Generic;
using System.Linq;
using ZaloCommunityDev.Shared.Structures;

namespace ZaloCommunityDev.Shared
{
    public static class Extensions
    {
        public static string Substring(this string @this, string from = null, string until = null, StringComparison comparison = StringComparison.InvariantCulture)
        {
            var fromLength = (from ?? string.Empty).Length;
            var startIndex = !string.IsNullOrEmpty(from)
                ? @this.IndexOf(from, comparison) + fromLength
                : 0;

            if (startIndex < fromLength) { throw new ArgumentException("from: Failed to find an instance of the first anchor"); }

            var endIndex = !string.IsNullOrEmpty(until)
            ? @this.IndexOf(until, startIndex, comparison)
            : @this.Length;

            if (endIndex < 0) { throw new ArgumentException("until: Failed to find an instance of the last anchor"); }

            var subString = @this.Substring(startIndex, endIndex - startIndex);
            return subString;
        }

        public static T ElementAtOrDefault<T>(this IEnumerable<T> v, int at, T @default = default(T))
        {
            if (v.Count() > at)
                return v.ElementAt(at);

            return @default;
        }

        public static bool IsValid(this Filter value, out string reason)
        {
            reason = null;
            switch (value.GenderSelection)
            {
                case GenderSelection.OnlyMale:
                    if (string.IsNullOrWhiteSpace(value.TextGreetingForMale))
                    {
                        reason = "Phải điền lời chào bạn nam";
                        return false;
                    }
                    break;
                case GenderSelection.OnlyFemale:
                    if (string.IsNullOrWhiteSpace(value.TextGreetingForFemale))
                    {
                        reason = "Phải điền lời chào bạn nữ";
                        return false;
                    }
                    break;
                case GenderSelection.Both:
                    if (string.IsNullOrWhiteSpace(value.TextGreetingForFemale))
                    {
                        reason = "Phải điền lời chào bạn nữ";
                        return false;
                    }
                    if (string.IsNullOrWhiteSpace(value.TextGreetingForMale))
                    {
                        reason = "Phải điền lời chào bạn nam";
                        return false;
                    }
                    break;
            }

            return true;
        }

        public static string[] ZaloSplitText(this string text)
        {
            return text.Split(";".ToArray());
        }

        public static bool IsValidProfile(this Filter filter, ProfileMessage profile, out string reason)
        {
            reason = null;
            if (!string.IsNullOrWhiteSpace(filter.FilterAgeRange))
            {
                var ages = filter.FilterAgeRange.Split(";-=_ ".ToArray());

                var from = int.Parse(ages[0]);
                var to = int.Parse(ages[1]);

                DateTime date;
                if (DateTime.TryParse(profile.BirthdayText, out date) && date.Year >= 1930)
                {
                    var profileAge = DateTime.Now.Year - date.Year;

                    if (from <= profileAge && profileAge <= to)
                    {
                    }
                    else
                    {
                        reason = $"Độ tuổi {profileAge} không trong khoảng [{from}-{to}]";

                        return false;
                    }
                }
                else if (!filter.FilterAgeRangeAcceptIfHidden)
                {
                    reason = "Không thấy tuổi";

                    return false;
                }
            }
            

            if (filter.GenderSelection != GenderSelection.Both)
            {
                if (filter.GenderSelection == GenderSelection.OnlyMale && profile.GenderValue() != Gender.Male)
                {
                    reason = $"Yêu cầu chọn nam, nhưng kết quả trả về là nữ";

                    return false;
                }

                if (filter.GenderSelection == GenderSelection.OnlyFemale && profile.GenderValue() != Gender.Female)
                {
                    reason = $"Yêu cầu chọn nữ, nhưng kết quả trả về là nam";

                    return false;
                }
            }

            if (!string.IsNullOrWhiteSpace(filter.ExcludePhoneNumbers))
            {
                var phoneNumbers = filter.ExcludePhoneNumbers.Split(";".ToArray());
                if (phoneNumbers.Contains(profile.PhoneNumber))
                {
                    reason = "Số đt có trong danh sách loại trừ";
                    return false;
                }
            }

            if (!string.IsNullOrWhiteSpace(filter.Locations))
            {

            }

            return true;
        }
    }
}