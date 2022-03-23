using NHibernateQueryViewer.Core;

using System;
using System.Collections.Generic;
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
                parameter.Type = Type.GetType($"System.{type}"); // TODO: hack, fix later
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

            if (parameter.Type == typeof(DateTime))
            {
                // NHibernate datetime: 2022-03-24T18:37:42.9553368+02:00
                // SQL Server datetime: 2022-03-14 16:09:07.043
                var datetime = DateTime.Parse(parameter.Value, null, DateTimeStyles.RoundtripKind);
                parameter.Value = datetime.ToString("yyyy-MM-dd HH:mm:ss.fff");
                parameter.Value = $"'{parameter.Value}'";
            }

            if (parameter.Type == typeof(DateTimeOffset))
            {
                // NHibernate datetimeoffset: 2022-03-23T17:30:00.0798130+00:00
                // SQL Server datetime: 2022-03-14 14:00:00.1800000 +00:00
                var offset = DateTimeOffset.Parse(parameter.Value, null, DateTimeStyles.RoundtripKind);
                parameter.Value = offset.ToString("yyyy-MM-dd HH:mm:ss.fffffff zzz");
                parameter.Value = $"'{parameter.Value}'";
            }
        }
    }

    internal class Parameter
    {
        public string Key { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public Type? Type { get; set; }
    }
}
