using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using Parser.Core.Extensions;

namespace Parser.Core
{
    public class CsvParser
    {
        private CsvSchemaField[] m_Schema = new CsvSchemaField[0];
        private readonly CsvParserOptions m_Options;
        private static readonly Dictionary<Type, Converter<string, object>> m_DefaultConverters;

        static CsvParser()
        {
            m_DefaultConverters = new Dictionary<Type, Converter<string, object>>();
            registerConverters();
        }

        private static void registerConverters()
        {
            m_DefaultConverters[typeof(string)] = str => str.Trim();
            m_DefaultConverters[typeof(byte)] = str => byte.Parse(str, NumberStyles.Integer, CultureInfo.InvariantCulture);
            m_DefaultConverters[typeof(byte?)] = str => byte.Parse(str, NumberStyles.Integer, CultureInfo.InvariantCulture);
            m_DefaultConverters[typeof(int)] = str => int.Parse(str, NumberStyles.Integer, CultureInfo.InvariantCulture);
            m_DefaultConverters[typeof(int?)] = str => int.Parse(str, NumberStyles.Integer, CultureInfo.InvariantCulture);
            m_DefaultConverters[typeof(long)] = str => long.Parse(str, NumberStyles.Integer, CultureInfo.InvariantCulture);
            m_DefaultConverters[typeof(long?)] = str => long.Parse(str, NumberStyles.Integer, CultureInfo.InvariantCulture);
            m_DefaultConverters[typeof(double)] = str => double.Parse(str.Replace(",", "."), NumberStyles.Float, CultureInfo.InvariantCulture);
            m_DefaultConverters[typeof(double?)] = str => double.Parse(str.Replace(",", "."), NumberStyles.Float, CultureInfo.InvariantCulture);
            m_DefaultConverters[typeof(float)] = str => float.Parse(str.Replace(",", "."), NumberStyles.Float, CultureInfo.InvariantCulture);
            m_DefaultConverters[typeof(float?)] = str => float.Parse(str.Replace(",", "."), NumberStyles.Float, CultureInfo.InvariantCulture);
            m_DefaultConverters[typeof(bool)] = str => bool.Parse(str.Replace("1", "true").Replace("0", "false"));
            m_DefaultConverters[typeof(bool?)] = str => bool.Parse(str.Replace("1", "true").Replace("0", "false"));
            m_DefaultConverters[typeof(DateTime)] = str => DateTime.Parse(str, CultureInfo.InvariantCulture, DateTimeStyles.None);
            m_DefaultConverters[typeof(DateTime?)] = str => DateTime.Parse(str, CultureInfo.InvariantCulture, DateTimeStyles.None);
            m_DefaultConverters[typeof(Guid)] = str => Guid.Parse(str);
            m_DefaultConverters[typeof(Guid?)] = str => Guid.Parse(str);
        }

        public CsvParser(CsvParserOptions options, params CsvSchemaField[] schema)
            : this(options)
        {
            if (schema == null) throw new ArgumentNullException("schema");

            SetSchema(schema);
        }

        internal CsvParser(CsvParserOptions options)
        {
            if (options == null) throw new ArgumentNullException("options");

            m_Options = options.Merge(CsvParserOptions.Default);
        }

        internal void SetSchema(params CsvSchemaField[] schema)
        {
            if (schema == null) throw new ArgumentNullException("schema");
            m_Schema = schema;
        }       

        public IEnumerable<T> Parse<T>(IEnumerable<string> lines) where T : new()
        {
            EnsureSchema();

            int counter = 0;
            bool firstNotIgnoredLine = true;
            foreach (var line in lines)
            {
                ++counter;
                if (m_Options.IgnoreLineDetector(line)) continue;
                if (firstNotIgnoredLine)
                {
                    firstNotIgnoredLine = false;
                    if (m_Options.HasHeader) continue;
                }

                yield return ParseCore<T>(counter, line);
            }
        }

        public IEnumerable<T> Parse<T>(TextReader reader) where T : new()
        {
            EnsureSchema();

            string line;
            int counter = 0;
            bool firstNotIgnoredLine = true;
            while ((line = reader.ReadLine()) != null)
            {
                ++counter;
                if (m_Options.IgnoreLineDetector(line)) continue;
                if (firstNotIgnoredLine)
                {
                    firstNotIgnoredLine = false;
                    if (m_Options.HasHeader) continue;
                }
                yield return ParseCore<T>(counter, line);
            }
        }

        protected T ParseCore<T>(int lineNumber, string csvLine) where T : new()
        {
            if (csvLine == null) throw new ArgumentNullException("csvLine");

            var parts = m_Options.Tokenizer(csvLine);

            if (parts.Length < m_Schema.Max(x => x.Position) + 1)
                throw new CsvParserException(lineNumber,
                    string.Format("Line '{0}' contains too few fields (actual: {1}, expected: {2})", csvLine, parts.Length, m_Schema.Max(x => x.Position) + 1));

            var parsed = new T();

            foreach (var field in m_Schema)
            {
                var rawFieldValue = parts[field.Position];
                object fieldValue;
                try
                {
                    fieldValue = ParseFieldValue(rawFieldValue, field.Type);
                }
                catch (Exception ex)
                {
                    throw new CsvParserException(lineNumber, string.Format("Failed to parse value of the field {0} of type {1} from string '{2}'", field.Name, field.Type, rawFieldValue), ex);
                }

                try
                {
                    SetFieldOrPropertyValue(parsed, field.Name, fieldValue);
                }
                catch (Exception ex)
                {
                    throw new CsvParserException(lineNumber, string.Format("Failed set value of the field {0} of type {1} to {2} ({3})", field.Name, field.Type, fieldValue, fieldValue == null ? "null" : fieldValue.GetType().Name), ex);
                }
            }

            return parsed;
        }
        
        [System.Diagnostics.DebuggerStepThrough]
        protected void EnsureSchema()
        {
            if (m_Schema == null || m_Schema.Length == 0) throw new ConfigurationException("CSV shema is not set");
        } 

        protected object ParseFieldValue(string rawFieldValue, Type asType)
        {
            if (asType == null) throw new ArgumentNullException("asType");

            if (m_Options.NullValueDetector(rawFieldValue, asType))
            {
                return asType.GetDefaultValue();
            }

            Converter<string, object> converter;

            if (m_Options.Converters.TryGetValue(asType, out converter))
            {
                return converter(rawFieldValue);
            }

            if (m_DefaultConverters.TryGetValue(asType, out converter))
            {
                return converter(rawFieldValue);
            }

            return m_Options.UnknownTypeParser(rawFieldValue, asType);
        }

        readonly Dictionary<Tuple<Type, string>, Action<object, object>> m_SettersCache = new Dictionary<Tuple<Type, string>, Action<object, object>>();

        protected void SetFieldOrPropertyValue(object container, string fieldName, object fieldValue)
        {
            Action<object, object> setter;
            var key = Tuple.Create(container.GetType(), fieldName);
            if (!m_SettersCache.TryGetValue(key, out setter))
            {
                m_SettersCache[key] = setter = container.GetType().CreateSetter(fieldName);
            }
            setter(container, fieldValue);
        }
    }
}