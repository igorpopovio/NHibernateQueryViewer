namespace NHibernateQueryViewer.Core.Filters;

using System;

public class FilterSavedEventArgs : EventArgs
{
    public Filter Filter { get; set; } = Filter.Empty;
}
