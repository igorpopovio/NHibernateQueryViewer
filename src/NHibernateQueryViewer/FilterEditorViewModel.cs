namespace NHibernateQueryViewer;

using NHibernateQueryViewer.Core;

using System;

public class FilterEditorViewModel : ViewModel
{
    public event EventHandler? CloseView;

    public string IncludeKeywords { get; set; } = string.Empty;

    public string ExcludeKeywords { get; set; } = string.Empty;

    public void Save()
    {
        // TODO: implement saving
        DoCloseView();
    }

    public void Cancel() => DoCloseView();

    public void DoCloseView() => CloseView?.Invoke(this, EventArgs.Empty);
}
