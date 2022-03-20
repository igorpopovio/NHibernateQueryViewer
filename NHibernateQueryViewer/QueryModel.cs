using NHibernateQueryViewer.Core;

namespace NHibernateQueryViewer
{
    public class QueryModel : ObservableObject
    {
        public string? ShortForm => RawQuery;
        public string? RawQuery { get; set; }
        public string? DisplayQuery { get; set; }
        public string Language { get; set; }
    }
}
