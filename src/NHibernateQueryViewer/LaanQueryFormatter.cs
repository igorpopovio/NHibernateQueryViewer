namespace NHibernateQueryViewer;

using Laan.Sql.Formatter;

using NHibernateQueryViewer.Core.Queries;

public class LaanQueryFormatter : IQueryFormatter
{
    private readonly FormattingEngine _formatter;

    public LaanQueryFormatter()
    {
        _formatter = new FormattingEngine();
    }

    public string Format(string query)
    {
        return _formatter.Execute(query);
    }
}
