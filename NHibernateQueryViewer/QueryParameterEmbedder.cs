using NHibernateQueryViewer.Core;

using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace NHibernateQueryViewer
{
    public class QueryParameterEmbedder : IQueryParameterEmbedder
    {
        private static readonly Regex _queryParameterRegex = new(
            @"(?<key>@p\d+)\s+=\s+(?<value>.+?)\s+\[Type:\s+(?<type>\w+)",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        // parameter name uses a "name prefix":
        // Sql Server uses "@" and Oracle uses ":"

        /*
        example: [Type: DateTime (10:0:0)]
        meaning: [Type: DbType (Size:Scale:Precision)]
        source: SqlStatementLogger.GetParameterLoggableType(DbParameter dataParameter)
        link: https://github.com/nhibernate/nhibernate-core/blob/master/src/NHibernate/AdoNet/Util/SqlStatementLogger.cs
        code:
        private static string GetParameterLoggableType(DbParameter dataParameter)
        {
            return dataParameter.DbType + " (" + dataParameter.Size + ":" + dataParameter.Scale + ":" + dataParameter.Precision + ")";
        }
         */

        public string Embed(string queryWithParameters)
        {
            var queries = queryWithParameters.Split(";\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).ToList();
            var parameters = LoadParametersFrom(queries.Last());
            if (parameters.Any())
                queries = queries.SkipLast(1).ToList();
            var rawQuery = string.Join(Environment.NewLine, queries.ToArray());

            var finalQuery = new StringBuilder(rawQuery);

            parameters.Reverse();
            foreach (var parameter in parameters)
                finalQuery = finalQuery.Replace(parameter.Key, parameter.Value);

            return finalQuery.ToString();
        }

        private List<Parameter> LoadParametersFrom(string? input)
        {
            var parameters = new List<Parameter>();
            var matches = _queryParameterRegex.Matches(input);

            foreach (Match match in matches)
            {
                var groups = match.Groups;

                var parameter = new Parameter();
                var type = groups["type"].Value;
                parameter.Type = (DbType)Enum.Parse(typeof(DbType), type);
                parameter.Key = groups["key"].Value;
                parameter.Value = groups["value"].Value;

                HandleSpecialCases(parameter);

                parameters.Add(parameter);
            }

            return parameters;
        }

        private void HandleSpecialCases(Parameter parameter)
        {
            if (parameter.Value.ToUpper() == "NULL") return;

            if (parameter.Type == DbType.Boolean)
            {
                parameter.Value = bool.Parse(parameter.Value) ? "1" : "0";
            }

            if (parameter.Type == DbType.Guid)
            {
                parameter.Value = $"'{parameter.Value}'";
            }

            if (parameter.Type == DbType.DateTime || parameter.Type == DbType.DateTime2)
            {
                var datetime = DateTime.Parse(parameter.Value, null, DateTimeStyles.RoundtripKind);
                parameter.Value = datetime.ToString("yyyy-MM-dd HH:mm:ss.fff");
                parameter.Value = $"'{parameter.Value}'";
            }

            if (parameter.Type == DbType.DateTimeOffset)
            {
                var offset = DateTimeOffset.Parse(parameter.Value, null, DateTimeStyles.RoundtripKind);
                parameter.Value = offset.ToString("yyyy-MM-dd HH:mm:ss.fffffff zzz");
                parameter.Value = $"'{parameter.Value}'";
            }
        }
    }

    // TODO: could use DbParameter or SqlParameter or just trimmed down version below
    internal class Parameter
    {
        public string Key { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public DbType Type { get; set; }
        // DbType/DbType
        // Size/int
        // Scale/byte
        // Precision/byte
        // ParameterName/string
        // Value/object/string
    }
}
