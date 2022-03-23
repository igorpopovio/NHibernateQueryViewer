using NHibernateQueryViewer;

using NUnit.Framework;

using System.Text;

namespace Tests
{
    public class Tests
    {
        [Test]
        public void CanEmbedParametersInQuery()
        {
            var parser = new QueryParameterEmbedder();
            var sb = new StringBuilder();
            sb.Append("SELECT this_.Id as y0_ FROM Person this_ WHERE Id = @p1;");
            sb.Append("SELECT last_insert_rowid();");
            sb.Append("@p1 = 1 [Type: Int32 (0:0:0)]");

            var query = parser.Embed(sb.ToString());

            Assert.That(query, Contains.Substring("SELECT this_.Id as y0_ FROM Person this_ WHERE Id = 1"));
            Assert.That(query, Contains.Substring("SELECT last_insert_rowid()"));
        }

        [Test]
        public void EmbedsParametersInReverseOrder()
        {
            var parser = new QueryParameterEmbedder();
            var sb = new StringBuilder();
            sb.Append("SELECT this_.Id as y0_ FROM Person this_ WHERE Id IN (@p1, @p1234);");
            sb.Append("@p1 = 1 [Type: Int32 (0:0:0)], @p1234 = 2 [Type: Int32 (0:0:0)]");

            var query = parser.Embed(sb.ToString());

            Assert.That(query, Is.EqualTo("SELECT this_.Id as y0_ FROM Person this_ WHERE Id IN (1, 2)"));
        }

        [Test]
        public void EmbedsDateTimeParameters()
        {
            var parser = new QueryParameterEmbedder();
            var sb = new StringBuilder();
            sb.Append("INSERT INTO Admin_ErrorLog (RecordDateTime) VALUES (@p0);");
            sb.Append("@p0 = 2022-03-24T18:37:42.9553368+02:00 [Type: DateTime (10:0:0)]");

            var query = parser.Embed(sb.ToString());

            Assert.That(query, Is.EqualTo("INSERT INTO Admin_ErrorLog (RecordDateTime) VALUES ('2022-03-24 18:37:42.955')"));
        }

        [Test]
        public void EmbedsDateTimeOffsetParameters()
        {
            var parser = new QueryParameterEmbedder();
            var sb = new StringBuilder();
            sb.Append("INSERT INTO Admin_ErrorLog (RecordDateTime) VALUES (@p0);");
            sb.Append("@p0 = 2022-03-23T17:30:00.0798130+00:00 [Type: DateTimeOffset (10:0:0)]");

            var query = parser.Embed(sb.ToString());

            Assert.That(query, Is.EqualTo("INSERT INTO Admin_ErrorLog (RecordDateTime) VALUES ('2022-03-23 17:30:00.0798130 +00:00')"));
        }

        [Test]
        public void EmbedsNullParameters()
        {
            var parser = new QueryParameterEmbedder();
            var sb = new StringBuilder();
            sb.Append("INSERT INTO Admin_Session (RecordDateTime) VALUES (@p0);");
            sb.Append("@p0 = NULL [Type: DateTimeOffset (10:0:0)]");

            var query = parser.Embed(sb.ToString());

            Assert.That(query, Is.EqualTo("INSERT INTO Admin_Session (RecordDateTime) VALUES (NULL)"));
        }

        [Test]
        public void EmbedsBooleanParameters()
        {
            var parser = new QueryParameterEmbedder();
            var sb = new StringBuilder();
            sb.Append("INSERT INTO Users (IsSystem, IsDefault) VALUES (@p0, @p1);");
            sb.Append("@p0 = True [Type: Boolean (0:0:0)]");
            sb.Append("@p1 = False [Type: Boolean (0:0:0)]");

            var query = parser.Embed(sb.ToString());

            Assert.That(query, Is.EqualTo("INSERT INTO Users (IsSystem, IsDefault) VALUES (1, 0)"));
        }

        [Test]
        public void EmbedsGuidParameters()
        {
            var parser = new QueryParameterEmbedder();
            var sb = new StringBuilder();
            sb.Append("INSERT INTO Admin_ServerRequestLogger (ServerRequestId) VALUES (@p0);");
            sb.Append("@p0 = 1add7a47-e0ae-4017-8338-be8d271fbd69 [Type: Guid (10:0:0)]");

            var query = parser.Embed(sb.ToString());

            Assert.That(query, Is.EqualTo("INSERT INTO Admin_ServerRequestLogger (ServerRequestId) VALUES ('1add7a47-e0ae-4017-8338-be8d271fbd69')"));
        }
    }
}
