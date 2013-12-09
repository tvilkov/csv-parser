using System;

namespace Parser.Core.Extensions
{
    public static class StringEntensions
    {
        public static string CutEnd(this string str, string part)
        {
            if (str == null) throw new ArgumentNullException("str");
            if (part == null) throw new ArgumentNullException("part");

            return str.EndsWith(part) ? str.Substring(0, str.Length - part.Length) : str;
        }

        public static string CutStart(this string str, string part)
        {
            if (str == null) throw new ArgumentNullException("str");
            if (part == null) throw new ArgumentNullException("part");

            return str.StartsWith(part) ? str.Substring(part.Length, str.Length - part.Length) : str;
        }
    }
}