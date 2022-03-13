using System;
using System.Collections.Generic;
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
            @"(?<key>@p\d+)\s+=\s+(?<value>.+?)\s+\[",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

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
            foreach (var (key, value) in parameters.Reverse())
                finalQuery = finalQuery.Replace(key, value);
            query.WithParameters = finalQuery.ToString();

            return query;
        }

        private Dictionary<string, string> LoadParametersFrom(string? input)
        {
            var parameters = new Dictionary<string, string>();
            var matches = _queryParameterRegex.Matches(input);

            foreach (Match match in matches)
            {
                var groups = match.Groups;
                var key = groups["key"].Value;
                var value = groups["value"].Value;
                parameters[key] = value;
            }

            return parameters;
        }
    }
}
