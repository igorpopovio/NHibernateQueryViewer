namespace Tests;

using NHibernateQueryViewer;

using NUnit.Framework;

using System;
using System.Text;

public class QueryParameterEmbedderTests
{
    private QueryParameterEmbedder _embedder;
    private StringBuilder _rawQuery;

    [SetUp]
    public void SetUp()
    {
        _embedder = new QueryParameterEmbedder();
        _rawQuery = new StringBuilder();
    }

    [Test]
    public void EmbedsIntegerParameters()
    {
        _rawQuery.Append("SELECT Id FROM Person WHERE Id = @p0;");
        _rawQuery.Append("@p0 = 1 [Type: Int32 (0:0:0)]");

        var query = _embedder.Embed(_rawQuery.ToString());

        Assert.That(query, Is.EqualTo("SELECT Id FROM Person WHERE Id = 1"));
    }

    [Test]
    public void EmbedsDoubleParameters()
    {
        _rawQuery.Append("SELECT Id FROM Person WHERE Height > @p0;");
        _rawQuery.Append("@p0 = 1.83 [Type: Double (0:0:0)]");

        var query = _embedder.Embed(_rawQuery.ToString());

        Assert.That(query, Is.EqualTo("SELECT Id FROM Person WHERE Height > 1.83"));
    }

    [Test]
    public void EmbedsParametersWithSimilarNames()
    {
        _rawQuery.Append("SELECT Id FROM Person WHERE Id IN (@p123, @p1234);");
        _rawQuery.Append("@p123 = 1 [Type: Int32 (0:0:0)], @p1234 = 2 [Type: Int32 (0:0:0)]");

        var query = _embedder.Embed(_rawQuery.ToString());

        Assert.That(query, Is.EqualTo("SELECT Id FROM Person WHERE Id IN (1, 2)"));
    }

    [TestCase("DateTime")]
    [TestCase("DateTime2")]
    public void EmbedsDateTimeParameters(string dateTimeType)
    {
        var localDateTime = new DateTime(2022, 3, 24, 18, 37, 42, 955, DateTimeKind.Local);
        var roundTripDateTime = localDateTime.ToString("O");
        _rawQuery.Append("INSERT INTO Admin_ErrorLog (RecordDateTime) VALUES (@p0);");
        _rawQuery.Append($"@p0 = {roundTripDateTime} [Type: {dateTimeType} (10:0:0)]");

        var query = _embedder.Embed(_rawQuery.ToString());

        Assert.That(query, Is.EqualTo("INSERT INTO Admin_ErrorLog (RecordDateTime) VALUES ('2022-03-24 18:37:42.955')"));
    }

    [Test]
    public void EmbedsDateTimeOffsetParameters()
    {
        _rawQuery.Append("INSERT INTO Admin_ErrorLog (RecordDateTime) VALUES (@p0);");
        _rawQuery.Append("@p0 = 2022-03-23T17:30:00.0798130+00:00 [Type: DateTimeOffset (10:0:0)]");

        var query = _embedder.Embed(_rawQuery.ToString());

        Assert.That(query, Is.EqualTo("INSERT INTO Admin_ErrorLog (RecordDateTime) VALUES ('2022-03-23 17:30:00.0798130 +00:00')"));
    }

    [Test]
    public void EmbedsNullParameters()
    {
        _rawQuery.Append("INSERT INTO Admin_Session (RecordDateTime) VALUES (@p0);");
        _rawQuery.Append("@p0 = NULL [Type: DateTimeOffset (10:0:0)]");

        var query = _embedder.Embed(_rawQuery.ToString());

        Assert.That(query, Is.EqualTo("INSERT INTO Admin_Session (RecordDateTime) VALUES (NULL)"));
    }

    [Test]
    public void EmbedsBooleanParameters()
    {
        _rawQuery.Append("INSERT INTO Users (IsSystem, IsDefault) VALUES (@p0, @p1);");
        _rawQuery.Append("@p0 = True [Type: Boolean (0:0:0)]");
        _rawQuery.Append("@p1 = False [Type: Boolean (0:0:0)]");

        var query = _embedder.Embed(_rawQuery.ToString());

        Assert.That(query, Is.EqualTo("INSERT INTO Users (IsSystem, IsDefault) VALUES (1, 0)"));
    }

    [Test]
    public void EmbedsGuidParameters()
    {
        _rawQuery.Append("INSERT INTO Admin_ServerRequestLogger (ServerRequestId) VALUES (@p0);");
        _rawQuery.Append("@p0 = 1add7a47-e0ae-4017-8338-be8d271fbd69 [Type: Guid (10:0:0)]");

        var query = _embedder.Embed(_rawQuery.ToString());

        Assert.That(query, Is.EqualTo("INSERT INTO Admin_ServerRequestLogger (ServerRequestId) VALUES ('1add7a47-e0ae-4017-8338-be8d271fbd69')"));
    }

    [Test]
    public void EmbedsStringParameters()
    {
        _rawQuery.Append("INSERT INTO Admin_ServerRequestLogger (Message) VALUES (@p0);");
        _rawQuery.Append("@p0 = 'Simple message' [Type: String (0:0:0)]");

        var query = _embedder.Embed(_rawQuery.ToString());

        Assert.That(query, Is.EqualTo("INSERT INTO Admin_ServerRequestLogger (Message) VALUES ('Simple message')"));
    }

    [Test]
    public void EmbedsStringsWithSemicolons()
    {
        _rawQuery.Append("INSERT INTO Admin_ServerRequestLogger (Message) VALUES (@p0);");
        _rawQuery.Append("@p0 = 'Message with ;@' [Type: String (0:0:0)]");

        var query = _embedder.Embed(_rawQuery.ToString());

        Assert.That(query, Is.EqualTo("INSERT INTO Admin_ServerRequestLogger (Message) VALUES ('Message with ;@')"));
    }

    [Test]
    public void EmbedsParametersInAllQueries()
    {
        _rawQuery.Append("SELECT Id FROM Person WHERE Id = @p0;");
        _rawQuery.Append("SELECT Id FROM Pet WHERE Name = @p1;");
        _rawQuery.Append("@p0 = 1 [Type: Int32 (0:0:0)],");
        _rawQuery.Append("@p1 = 'Max' [Type: String (0:0:0)]");

        var query = _embedder.Embed(_rawQuery.ToString());

        Assert.That(query, Contains.Substring("SELECT Id FROM Person WHERE Id = 1"));
        Assert.That(query, Contains.Substring("SELECT Id FROM Pet WHERE Name = 'Max'"));
    }

    [Test]
    public void EmbedsMultilineParameters()
    {
        _rawQuery.Append("INSERT INTO Admin_ServerRequestLogger (Message) VALUES (@p0);");
        _rawQuery.Append("@p0 = 'line1\nline2\r' [Type: String (0:0:0)]");

        var query = _embedder.Embed(_rawQuery.ToString());

        Assert.That(query, Is.EqualTo("INSERT INTO Admin_ServerRequestLogger (Message) VALUES ('line1\nline2\r')"));
    }

    [TestCase(0, 1)]
    [TestCase(1, 2)]
    public void CommentsOffsetStatements(int offset, int row)
    {
        // limitation from underlying SQL formatter
        // either change lib or comment out statements
        // that crash it:
        // OFFSET 0 ROWS FETCH FIRST 1 ROWS ONLY
        _rawQuery.Append($"SELECT Id FROM Person WHERE Id = @p0 OFFSET {offset} ROWS FETCH FIRST {row} ROWS ONLY");

        var query = _embedder.Embed(_rawQuery.ToString());

        Assert.That(query, Is.EqualTo($"SELECT Id FROM Person WHERE Id = @p0 -- OFFSET {offset} ROWS FETCH FIRST {row} ROWS ONLY"));
    }
}
