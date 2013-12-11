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
        public Func<string, Type, bool> NullValueDetector;
        public Func<string, bool> IgnoreLineDetector;

        public CsvParserOptions Merge(CsvParserOptions other)
        {
            return new CsvParserOptions
                {
                    HasHeader = HasHeader,
                    Tokenizer = Tokenizer ?? other.Tokenizer,
                    NullValueDetector = NullValueDetector ?? other.NullValueDetector,
                    UnknownTypeParser = UnknownTypeParser ?? other.UnknownTypeParser,
                    IgnoreLineDetector = IgnoreLineDetector ?? other.IgnoreLineDetector,
                    Converters = Converters ?? other.Converters,
                };
        }

        internal static readonly CsvParserOptions Default = new CsvParserOptions
            {
                HasHeader = false,
                Tokenizer = str => str.Trim().CutEnd(";").Split(';'),
                NullValueDetector = (str, type) => string.IsNullOrWhiteSpace(str),
                UnknownTypeParser = (str, type) => Convert.ChangeType(str, type, CultureInfo.InvariantCulture),
                IgnoreLineDetector = str => string.IsNullOrWhiteSpace(str) || str.StartsWith("#"),
                Converters = new Dictionary<Type, Converter<string, object>>(),
            };
    }
}