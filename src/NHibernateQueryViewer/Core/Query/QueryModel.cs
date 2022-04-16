namespace NHibernateQueryViewer.Core.Queries;

public class QueryModel : ObservableObject
{
    public string? ShortForm => RawQuery;

    public string? RawQuery { get; set; }

    public string? DisplayQuery { get; set; }

    public string Language { get; set; } = string.Empty;
}
