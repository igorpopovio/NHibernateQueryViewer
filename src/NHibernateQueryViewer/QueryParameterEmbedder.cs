namespace NHibernateQueryViewer;

using NHibernateQueryViewer.Core;

using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

public class QueryParameterEmbedder : IQueryParameterEmbedder
{
    private static readonly Regex _queryParameterRegex = new (
        @"(?<name>[@:]\w+)\s+=\s+(?<value>(.|\n|\r)+?)\s+\[Type:\s+(?<type>\w+)\s+\((?<size>\w+):(?<scale>\w+):(?<precision>\w+)\)\]",
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
        if (queryWithParameters == null)
        {
            throw new ArgumentNullException(nameof(queryWithParameters));
        }

        var indexOfQuery = queryWithParameters.IndexOf(";@", StringComparison.InvariantCulture);
        var rawQuery = indexOfQuery == -1 ? queryWithParameters : queryWithParameters[..indexOfQuery];
        var rawParameters = indexOfQuery == -1 ? string.Empty : queryWithParameters[indexOfQuery..];
        var parameters = LoadParametersFrom(rawParameters);
        var finalQuery = new StringBuilder(rawQuery);

        parameters.Reverse();
        foreach (var parameter in parameters)
        {
            finalQuery = finalQuery.Replace(parameter.Name, parameter.Value);
        }

        // remove part where parser crashes (either this or change parser/formatter)
        finalQuery = finalQuery.Replace("OFFSET", "-- OFFSET");

        return finalQuery.ToString();
    }

    private List<Parameter> LoadParametersFrom(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return new List<Parameter>();
        }

        var parameters = new List<Parameter>();
        var matches = _queryParameterRegex.Matches(input);

        foreach (Match match in matches)
        {
            var groups = match.Groups;
            var parameter = ExtractParameterFrom(groups);
            ConvertValuesFromDotNetToSqlServer(parameter);
            parameters.Add(parameter);
        }

        return parameters;
    }

    private Parameter ExtractParameterFrom(GroupCollection groups)
    {
        var parameter = new Parameter
        {
            Name = groups["name"].Value,
            Type = (DbType)Enum.Parse(typeof(DbType), groups["type"].Value),
            Value = groups["value"].Value,
            Size = int.Parse(groups["size"].Value, CultureInfo.InvariantCulture),
            Scale = byte.Parse(groups["scale"].Value, CultureInfo.InvariantCulture),
            Precision = byte.Parse(groups["precision"].Value, CultureInfo.InvariantCulture),
        };
        return parameter;
    }

    private void ConvertValuesFromDotNetToSqlServer(Parameter parameter)
    {
        if (parameter.Value.ToUpper(CultureInfo.InvariantCulture) == "NULL")
        {
            return;
        }

        if (parameter.Value.First() == '\'' && parameter.Value.Last() == '\'')
        {
            return;
        }

        if (decimal.TryParse(parameter.Value, out _))
        {
            return;
        }

        switch (parameter.Type)
        {
            case DbType.DateTime:
            case DbType.DateTime2:
                var datetime = DateTime.Parse(parameter.Value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
                parameter.Value = datetime.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture);
                parameter.Value = $"'{parameter.Value}'";
                break;
            case DbType.DateTimeOffset:
                var offset = DateTimeOffset.Parse(parameter.Value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
                parameter.Value = offset.ToString("yyyy-MM-dd HH:mm:ss.fffffff zzz", CultureInfo.InvariantCulture);
                parameter.Value = $"'{parameter.Value}'";
                break;
            case DbType.Boolean:
                parameter.Value = bool.Parse(parameter.Value) ? "1" : "0";
                break;

            default:
                parameter.Value = $"'{parameter.Value}'";
                break;
        }
    }
}

internal class Parameter
{
    // original code uses DbParameter/SqlParameter,
    // but we can use a simplified version here
    public string Name { get; set; } = string.Empty;

    public string Value { get; set; } = string.Empty;

    public DbType Type { get; set; }

    public int Size { get; set; }

    public byte Scale { get; set; }

    public byte Precision { get; set; }
}
