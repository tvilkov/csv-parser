using System;
using System.Runtime.Serialization;

namespace Parser.Core
{
    [Serializable]
    class CsvParserException : Exception
    {
        public int LineNumber { get; private set; }

        public CsvParserException(int line)
        {
            LineNumber = line;
        }

        public CsvParserException(int line, string message)
            : base(message)
        {
            LineNumber = line;
        }

        public CsvParserException(int line, string message, Exception innerException)
            : base(message, innerException)
        {
            LineNumber = line;
        }

        protected CsvParserException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}