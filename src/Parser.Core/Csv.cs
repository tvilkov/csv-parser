using System;
using System.Linq;
using System.Reflection;

namespace Parser.Core
{
    /// <summary>
    /// Convinient entry point to CSV parser
    /// </summary>
    public static class Csv
    {
        public static CsvParser Parser(CsvParserOptions options = null)
        {
            return new CsvParser(options ?? new CsvParserOptions());
        }

        public static CsvParser WithSchema(this CsvParser parser, params CsvSchemaField[] fields)
        {
            if (parser == null) throw new ArgumentNullException("parser");
            if (fields == null) throw new ArgumentNullException("fields");

            parser.SetSchema(fields);
            return parser;
        }

        public static CsvParser WithSchemaFromType(this CsvParser parser, Type recordType)
        {
            var schema = recordType.GetMembers(BindingFlags.Public | BindingFlags.Instance)
                            .Where(mi => mi.MemberType == MemberTypes.Field || mi.MemberType == MemberTypes.Property)
                            .Select((mi, position) => new CsvSchemaField(mi.Name, position, getFieldOrPropertyType(mi)));
            parser.SetSchema(schema.ToArray());
            return parser;
        }

        public static CsvSchemaField Field(string name, int position, Type type = null)
        {
            return new CsvSchemaField(name, position, type ?? typeof(string));
        }

        // ReSharper disable CanBeReplacedWithTryCastAndCheckForNull
        private static Type getFieldOrPropertyType(MemberInfo memberInfo)
        {
            if (memberInfo is PropertyInfo)
                return ((PropertyInfo)memberInfo).PropertyType;
            if (memberInfo is FieldInfo)
                return ((FieldInfo)memberInfo).FieldType;
            throw new ArgumentException("memberInfo must be a property or field", "memberInfo");
        }
        // ReSharper restore CanBeReplacedWithTryCastAndCheckForNull
    }
}