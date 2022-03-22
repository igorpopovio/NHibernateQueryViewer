using NHibernateQueryViewer.Core;

using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NHibernateQueryViewer
{
    public class MainViewModel : ViewModel
    {
        private IQueryFormatter _queryFormatter;
        private IQueryParameterEmbedder _queryParameterEmbedder;
        private IQueryConnection? _queryConnection;
        private readonly Func<IQueryConnection> _queryConnectionFactory;

        public ObservableCollection<QueryModel> Queries { get; set; }
        public QueryModel? SelectedQuery { get; set; }

        public bool IsCapturing { get; set; }
        public ViewOption ViewOption { get; set; }

        public MainViewModel(
            IQueryFormatter queryFormatter,
            IQueryParameterEmbedder queryParameterEmbedder,
            Func<IQueryConnection> queryConnectionFactory)
        {
            _queryFormatter = queryFormatter;
            _queryParameterEmbedder = queryParameterEmbedder;
            _queryConnectionFactory = queryConnectionFactory;

            Queries = new ObservableCollection<QueryModel>();
            ViewOption = ViewOption.Format;

            PropertyChanged += UpdateViewOptionForSelectedQuery;
            PropertyChanged += HandleConnections;
        }

        private void HandleConnections(object? sender, PropertyChangedEventArgs args)
        {
            if (args.PropertyName != nameof(IsCapturing)) return;

            if (IsCapturing)
            {
                _queryConnection = _queryConnectionFactory();
            }
            else
            {
                (_queryConnection as IDisposable)?.Dispose();
                _queryConnection = null;
            }
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

        private void UpdateViewOptionForSelectedQuery(object? sender, PropertyChangedEventArgs args)
        {
            if (args.PropertyName != nameof(SelectedQuery) && args.PropertyName != nameof(ViewOption)) return;
            if (SelectedQuery == null || SelectedQuery.RawQuery == null) return;

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

        public void SelectFirstQuery()
        {
            SelectedQuery = Queries?.FirstOrDefault();
        }
    }
}
