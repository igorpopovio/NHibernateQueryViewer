using Laan.Sql.Formatter;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace NHibernateQueryViewer
{
    public interface IQueryParser
    {
        QueryModel Parse(string line);
    }

    public class QueryParser : IQueryParser
    {
        private const string DateFormat = "yyyy-MM-dd HH:mm:ss,fff";

        private static readonly Regex _queryParameterRegex = new Regex(
            @"(?<key>@p\d+)\s+=\s+(?<value>.+?)\s+\[Type:\s+(?<type>\w+)",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private FormattingEngine _formatter;

        public QueryParser()
        {
            // TODO: inject dependencies
            _formatter = new FormattingEngine();
        }

        public QueryModel Parse(string line)
        {
            var query = new QueryModel();

            var parts = line.Split(";\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            query.DateTime = DateTime.ParseExact(parts.First(), DateFormat, null);
            var queries = parts.Skip(1);
            var parameters = LoadParametersFrom(parts.Last());
            if (parameters.Any())
                queries = queries.SkipLast(1);
            query.Parameterized = string.Join(Environment.NewLine, queries.ToArray());

            var finalQuery = new StringBuilder(query.Parameterized);

            parameters.Reverse();
            foreach (var parameter in parameters)
                finalQuery = finalQuery.Replace(parameter.Key, parameter.Value);
            query.WithParameters = finalQuery.ToString();

            query.WithParameters = _formatter.Execute(query.WithParameters);

            return query;
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
            if (parameter.Type == typeof(DateTime))
            {
                // NHibernate datetime: 2022-03-24T18:37:42.9553368+02:00
                // SQL Server datetime: 2022-03-14 16:09:07.043
                var datetime = DateTime.Parse(parameter.Value, null, DateTimeStyles.RoundtripKind);
                parameter.Value = datetime.ToString("yyyy-MM-dd HH:mm:ss.fff");
                parameter.Value = $"'{parameter.Value}'";
            }
        }
    }

    internal class Parameter
    {
        public string Key { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public Type Type { get; set; }
    }
}
