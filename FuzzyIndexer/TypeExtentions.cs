using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace FuzzyIndexer
{
    internal static class TypeExtentions
    {

        #region string

        public static string StripHTML(this String s)
        {
            return Regex.Replace(s, "<[^>]*?>", String.Empty, RegexOptions.IgnoreCase);
        }

        public static IEnumerable<string> Tokenize(this string s)
        {
            return Regex.Split(s, @"\W+").Where(t => t != "");
        }

        public static List<string> TokenizeWithPunctuation(this string s)
        {
            if (s.Length <= 1)
                return new List<string>{s};

            var tokens = new List<string>();
            var lastTokenBeginsAt = 0;
            for (var i = 1; i < s.Length; i++)
            {
                if (IsWordBoundary(s[i - 1], s[i]))
                {
                    tokens.Add(s.Substring(lastTokenBeginsAt, i - lastTokenBeginsAt));
                    lastTokenBeginsAt = i;
                }
            }
            return tokens;
        }

        private static bool IsWordBoundary(char ch1, char ch2)
        {
            return (char.IsLetterOrDigit(ch1) && !char.IsLetterOrDigit(ch2)) ||
                   (!char.IsLetterOrDigit(ch1) && char.IsLetterOrDigit(ch2));
        }

        #endregion

        #region IEnumerable

        public static T ElementAtMax<T>(this IEnumerable<T> collection, Func<T, int> selector)
        {
            var valueAtMax = int.MinValue;
            var elemAtMax = default(T);
            foreach (var elem in collection)
            {
                var value = selector(elem);
                if (value > valueAtMax)
                {
                    valueAtMax = value;
                    elemAtMax = elem;
                }
            }
            if (valueAtMax == int.MinValue)
                throw new Exception("IEnumerable.ElementAtMax: collection is empty!");
            return elemAtMax;
        }

        #endregion
    }
}
