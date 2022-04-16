namespace NHibernateQueryViewer.Core.Queries;

public class Query : ObservableObject
{
    public string? Raw { get; set; }

    public string? Enhanced { get; set; }

    public string? Language { get; set; }
}
