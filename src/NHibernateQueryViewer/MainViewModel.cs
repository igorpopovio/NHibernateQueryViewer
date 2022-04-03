﻿namespace NHibernateQueryViewer;

using NHibernateQueryViewer.Core;

using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

public class MainViewModel : ViewModel
{
    private readonly IQueryFormatter _queryFormatter;
    private readonly IQueryParameterEmbedder _queryParameterEmbedder;
    private readonly Func<IQueryConnection> _queryConnectionFactory;
    private IQueryConnection? _queryConnection;
    private string _filter = string.Empty;

    public MainViewModel(
        IQueryFormatter queryFormatter,
        IQueryParameterEmbedder queryParameterEmbedder,
        Func<IQueryConnection> queryConnectionFactory)
    {
        _queryFormatter = queryFormatter;
        _queryParameterEmbedder = queryParameterEmbedder;
        _queryConnectionFactory = queryConnectionFactory;
        Queries = new ObservableCollection<QueryModel>();
        FilteredQueries = CollectionViewSource.GetDefaultView(Queries);
        FilteredQueries.Filter = FilterQueries;
        ViewOption = ViewOption.Format;
        CaptureButtonName = "Capture";

        PropertyChanged += UpdateViewOptionForSelectedQuery;
        PropertyChanged += HandleConnections;
    }

    public event EventHandler? FocusFilter;

    public ObservableCollection<QueryModel> Queries { get; set; }

    public ICollectionView FilteredQueries { get; }

    public QueryModel? SelectedQuery { get; set; }

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

    public string CaptureButtonName { get; set; }

    public ViewOption ViewOption { get; set; }

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

    private bool FilterQueries(object obj)
    {
        if (string.IsNullOrWhiteSpace(Filter))
        {
            return true;
        }

        if (obj is not QueryModel query)
        {
            return false;
        }

        return query.RawQuery?.ToLower().Contains(Filter.ToLower()) ?? true;
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

        if (SelectedQuery == null || SelectedQuery.RawQuery == null)
        {
            return;
        }

        try
        {
            switch (ViewOption)
            {
                case ViewOption.Raw:
                    SelectedQuery.DisplayQuery = SelectedQuery.RawQuery;
                    break;
                case ViewOption.EmbedParameters:
                    SelectedQuery.DisplayQuery = _queryParameterEmbedder.Embed(SelectedQuery.RawQuery);
                    break;
                case ViewOption.Format:
                    var query = _queryParameterEmbedder.Embed(SelectedQuery.RawQuery);
                    SelectedQuery.DisplayQuery = _queryFormatter.Format(query);
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
            message.AppendLine($"Occured when using the **{ViewOption}** view option.");
            message.AppendLine();
            message.AppendLine("## Raw query");
            message.AppendLine(SelectedQuery.RawQuery);
            message.AppendLine("## Exception");
            message.AppendLine($"{exception.Message}{Environment.NewLine}{exception.StackTrace}");
            SelectedQuery.DisplayQuery = message.ToString();
            SelectedQuery.Language = "MarkDownWithFontSize";
        }
    }
}