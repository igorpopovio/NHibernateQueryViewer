namespace NHibernateQueryViewer.Core.Queries;

public class Query : ObservableObject
{
    public string Raw { get; set; } = string.Empty;

    public string Enhanced { get; set; } = string.Empty;

    public string Language { get; set; } = string.Empty;
}
