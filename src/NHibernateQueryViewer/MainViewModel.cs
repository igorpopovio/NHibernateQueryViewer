namespace NHibernateQueryViewer;

using NHibernateQueryViewer.Core;
using NHibernateQueryViewer.Core.Filters;
using NHibernateQueryViewer.Core.Queries;

using Stylet;

using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

public class MainViewModel : ViewModel
{
    private readonly IQueryFormatter _queryFormatter;
    private readonly IQueryParameterEmbedder _queryParameterEmbedder;
    private readonly Func<IQueryConnection> _queryConnectionFactory;
    private readonly IWindowManager _windowManager;
    private readonly FilterEditorViewModel _filterEditorViewModel;
    private readonly IFilterIO _filterIo;
    private readonly ITextFilter _textFilter;
    private IQueryConnection? _queryConnection;
    private string _filter = string.Empty;
    private Filter _advancedFilter;

    public MainViewModel(
        IQueryFormatter queryFormatter,
        IQueryParameterEmbedder queryParameterEmbedder,
        Func<IQueryConnection> queryConnectionFactory,
        IWindowManager windowManager,
        FilterEditorViewModel filterEditorViewModel,
        IFilterIO filterIo,
        ITextFilter textFilter)
    {
        _queryFormatter = queryFormatter;
        _queryParameterEmbedder = queryParameterEmbedder;
        _queryConnectionFactory = queryConnectionFactory;
        _windowManager = windowManager;
        _filterEditorViewModel = filterEditorViewModel;
        _filterIo = filterIo;
        _textFilter = textFilter;
        _advancedFilter = Core.Filters.Filter.Empty;
    }

    public event EventHandler? FocusFilter;

    public event EventHandler? SelectedQueryUpdated;

    public ObservableCollection<Query> Queries { get; } = new ObservableCollection<Query>();

    public ICollectionView FilteredQueries => CollectionViewSource.GetDefaultView(Queries);

    public Query? SelectedQuery { get; set; }

    public string Filter
    {
        get
        {
            return _filter;
        }

        set
        {
            _filter = value;
            FilteredQueries.Refresh();
            OnPropertyChange();
        }
    }

    public bool IsCapturing { get; set; }

    public string? CaptureButtonName { get; set; }

    public ViewOption ViewOption { get; set; }

    public async Task Initialize()
    {
        _advancedFilter = await _filterIo.Load();
        _filterIo.Saved += OnFilterKeywordsSaved;

        FilteredQueries.Filter = FilterQueries;

        ViewOption = ViewOption.Format;
        CaptureButtonName = "Capture";

        PropertyChanged += UpdateViewOptionForSelectedQuery;
        PropertyChanged += HandleConnections;
    }

    public async Task Capture()
    {
        try
        {
            while (IsCapturing && _queryConnection != null)
            {
                var query = await _queryConnection.ReceiveQueryAsync();
                Queries.Add(query);
            }
        }
        catch (ConnectionAbortedException)
        {
            // after closing the connection we still have a pending ReceiveQueryAsync
            // call which is no longer relevant so we can just ignore it
        }
    }

    public void Clear()
    {
        Queries.Clear();
        SelectedQuery = null;
    }

    public void DoFocusFilter()
    {
        // We want to prevent using UI components in the view model (MVVM),
        // so instead of setting the focus on the text box in here,
        // we invoke an event that the view/window will subscribe to.
        FocusFilter?.Invoke(this, EventArgs.Empty);
    }

    public void OpenFilterEditor()
    {
        _windowManager.ShowDialog(_filterEditorViewModel);
    }

    private bool FilterQueries(object obj)
    {
        if (obj is not Query query)
        {
            return false;
        }

        return _textFilter.Filter(query.Raw, Filter, _advancedFilter);
    }

    private void OnFilterKeywordsSaved(object? sender, FilterSavedEventArgs args)
    {
        _advancedFilter = args.Filter;
        FilteredQueries.Refresh();
    }

    private void HandleConnections(object? sender, PropertyChangedEventArgs args)
    {
        if (args.PropertyName != nameof(IsCapturing))
        {
            return;
        }

        if (IsCapturing)
        {
            _queryConnection = _queryConnectionFactory();
            CaptureButtonName = "Capturing";
        }
        else
        {
            (_queryConnection as IDisposable)?.Dispose();
            _queryConnection = null;
            CaptureButtonName = "Capture";
        }
    }

    private void UpdateViewOptionForSelectedQuery(object? sender, PropertyChangedEventArgs args)
    {
        if (args.PropertyName != nameof(SelectedQuery) && args.PropertyName != nameof(ViewOption))
        {
            return;
        }

        if (SelectedQuery == null || SelectedQuery.Raw == null)
        {
            return;
        }

        try
        {
            switch (ViewOption)
            {
                case ViewOption.Raw:
                    SelectedQuery.Enhanced = SelectedQuery.Raw;
                    break;
                case ViewOption.EmbedParameters:
                    SelectedQuery.Enhanced = _queryParameterEmbedder.Embed(SelectedQuery.Raw);
                    break;
                case ViewOption.Format:
                    var query = _queryParameterEmbedder.Embed(SelectedQuery.Raw);
                    SelectedQuery.Enhanced = _queryFormatter.Format(query);
                    break;
                default:
                    throw new InvalidOperationException($"Unrecognized ViewOption: {ViewOption}");
            }

            SelectedQuery.Language = "TSQL";
        }
        catch (Exception exception)
        {
            var message = new StringBuilder();
            message.AppendLine($"# Error");
            message.AppendLine(CultureInfo.InvariantCulture, $"Occured when using the **{ViewOption}** view option.");
            message.AppendLine();
            message.AppendLine("## Raw query");
            message.AppendLine(SelectedQuery.Raw);
            message.AppendLine("## Exception");
            message.AppendLine(CultureInfo.InvariantCulture, $"{exception.Message}{Environment.NewLine}{exception.StackTrace}");
            SelectedQuery.Enhanced = message.ToString();
            SelectedQuery.Language = "MarkDownWithFontSize";
        }

        SelectedQueryUpdated?.Invoke(this, EventArgs.Empty);
    }
}
