namespace NHibernateQueryViewer.Core.Filters;

using System;
using System.Threading.Tasks;

public interface IFilterIO
{
    public event EventHandler<FilterSavedEventArgs> Saved;

    Task Save(Filter filter);

    Task<Filter> Load();
}
