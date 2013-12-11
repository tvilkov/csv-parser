using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using NUnit.Framework;
using Parser.Core;

namespace Parser.Tests
{
    [TestFixture]
    public class ParserTests
    {
        [Test]
        public void CanParseWhenConfiguredExplicitlyTest()
        {
            var parser = Csv.Parser(new CsvParserOptions
            {
                NullValueDetector = (str, type) => string.IsNullOrWhiteSpace(str) || str.Trim() == "-"
            }).WithSchema(
                Csv.Field("Name", 0),
                Csv.Field("Age", 1, typeof(int)),
                Csv.Field("DateOfBirth", 2, typeof(DateTime)),
                Csv.Field("AnnualSallary", 3, typeof(double)),
                Csv.Field("Single", 4, typeof(bool))
            );

            var records = parser.Parse<Record>(TestData).ToArray();

            printRecords(records);

            Assert.AreEqual(5, records.Length);
        }

        [Test]
        public void CanParseWhenConfiguredFromSchemaTest()
        {
            var parser = Csv.Parser(new CsvParserOptions
                {
                    NullValueDetector = (str, type) => string.IsNullOrWhiteSpace(str) || str.Trim() == "-"
                }).WithSchemaFromType(typeof(Record));
            var records = parser.Parse<Record>(TestData).ToArray();

            printRecords(records);

            Assert.AreEqual(5, records.Length);
        }

        private static void printRecords(IEnumerable<Record> records)
        {
            foreach (var record in records)
            {
                Debug.WriteLine(record.ToString());
            }

        }

        private static IEnumerable<string> TestData
        {
            get
            {
                yield return "Adam;38;01.01.2013;5000.890;false";
                yield return "Eva;18;10.01.956;5000.890;1";
                yield return "Ivan;18;-;-45,89;true";
                yield return "Mike;36;12.01.2015;;;";
                yield return "Hoolio Rodriges;35;5.08.2013;123.456;1;";

            }
        }
    }

    class Record
    {
#pragma warning disable 649
        public string Name { get; set; }
        public int Age { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public double AnnualSallary;
        public bool Single;
#pragma warning restore 649

        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "{0} {1} {2:dd.MM.yyyy} {3:0.0000} {4}", Name, Age, DateOfBirth, AnnualSallary, Single);
        }
    }
}