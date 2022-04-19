namespace NHibernateQueryViewer.Core.Filters;

public interface ITextFilter
{
    bool Filter(string text, string mainFilter, Filter advancedFilter);
}
