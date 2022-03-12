using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace NHibernateQueryViewer
{
    public class MainViewModel : ViewModel
    {
        private const string DateFormat = "yyyy-MM-dd HH:mm:ss,fff";

        private readonly Regex _queryParameterRegex = new Regex(
            @"(?<key>@p\d+)\s+=\s+(?<value>.+?)\s+\[",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);
        public string Input { get; set; } = string.Empty;
        public string Output { get; set; } = string.Empty;

        public MainViewModel()
        {
            PropertyChanged += EmbedQueryParameters;
        }

        private void EmbedQueryParameters(object? sender, PropertyChangedEventArgs args)
        {
            if (args.PropertyName != nameof(Input)) return;

            try
            {
                EmbedQueryParametersInternal();
            }
            catch
            {
                var message = new StringBuilder();
                message.AppendLine("Error. Please enter a query like the following:");
                message.AppendLine("SELECT Id FROM Person WHERE Id = @p1;@p1 = 1 [Type: Int32 (0,0,0)];");
                Output = message.ToString();
            }
        }

        private void EmbedQueryParametersInternal()
        {
            var parts = Input.Split(";\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            var date = DateTime.ParseExact(parts.First(), DateFormat, null);
            var parameters = LoadParametersFrom(parts.Last());
            var queries = parts.Skip(1).SkipLast(1);


            var finalQueries = new List<string>();
            foreach (var query in queries)
            {
                var finalQuery = BuildQuery(query, parameters);
                finalQueries.Add(finalQuery);
            }

            Output = string.Join(Environment.NewLine, finalQueries.ToArray());
        }

        private string BuildQuery(string query, Dictionary<string, string> parameters)
        {
            var finalQuery = new StringBuilder(query);
            foreach (var (key, value) in parameters.Reverse())
                finalQuery = finalQuery.Replace(key, value);
            return finalQuery.ToString();
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
