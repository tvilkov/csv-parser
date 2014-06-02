CSV Parser
=========

Overview
----
CSV Parser is small and simple library for parsing CSV-format (Comma Separated Values) files.

Sample
----
```csharp
var parser = Csv.Parser(new CsvParserOptions
    {
        HasHeader = true,
        NullValueDetector = (str, type) => string.IsNullOrWhiteSpace(str) || str.Trim() == "-"
    })
    .WithSchemaFromType(typeof(Record));

using (var reader = new StreamReader(File.OpenRead("CorrectData.csv")))
{
    var records = parser.Parse<Record>(reader).ToArray();
    // Process records
}
```