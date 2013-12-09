using System;
using System.Collections.Generic;
using System.Globalization;
using Parser.Core.Extensions;

namespace Parser.Core
{
    public sealed class CsvParserOptions
    {
        public bool HasHeader;
        public Dictionary<Type, Converter<string, object>> Converters;
        public Func<string, string[]> Tokenizer;
        public Func<string, Type, object> UnknownTypeParser;
        public Func<string, Type, bool> IsNullValueString;

        internal static readonly CsvParserOptions Default = new CsvParserOptions
            {
                HasHeader = false,
                Tokenizer = (str => str.Trim().CutEnd(";").Split(';')),
                IsNullValueString = (str, type) => string.IsNullOrWhiteSpace(str),
                UnknownTypeParser = (str, type) => Convert.ChangeType(str, type, CultureInfo.InvariantCulture),
                Converters = new Dictionary<Type, Converter<string, object>>(),
            };
    }
}