using System;
using System.Diagnostics;

namespace Parser.Core
{
    [DebuggerDisplay("Name={Name} Position={Position} Type={Type}")]
    public sealed class CsvSchemaField
    {
        public readonly string Name;
        public readonly int Position;
        public readonly Type Type;

        internal CsvSchemaField(string name, int position, Type type)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("name should be non-empty string", "name");
            if (position < 0) throw new ArgumentException("position should be positive value", "position");
            if (type == null) throw new ArgumentNullException("type");

            Name = name;
            Position = position;
            Type = type;
        }
    }
}