namespace NHibernateQueryViewer;

using NHibernateQueryViewer.Core;
using NHibernateQueryViewer.Core.Filters;

using System;
using System.Threading.Tasks;

public class FilterEditorViewModel : ViewModel
{
    private readonly IFilterIO _filterIo;

    public FilterEditorViewModel(IFilterIO filterIo)
    {
        _filterIo = filterIo;
        _ = Initialize();
    }

    public event EventHandler? CloseView;

    public string IncludeKeywordsText { get; set; } = string.Empty;

    public string ExcludeKeywordsText { get; set; } = string.Empty;

    public async Task Initialize()
    {
        var filter = await _filterIo.Load();
        IncludeKeywordsText = filter.IncludeKeywordsText;
        ExcludeKeywordsText = filter.ExcludeKeywordsText;
    }

    public async Task Save()
    {
        var filter = new Filter(IncludeKeywordsText, ExcludeKeywordsText);
        await _filterIo.Save(filter);
        DoCloseView();
    }

    public void Cancel() => DoCloseView();

    public void DoCloseView() => CloseView?.Invoke(this, EventArgs.Empty);
}
