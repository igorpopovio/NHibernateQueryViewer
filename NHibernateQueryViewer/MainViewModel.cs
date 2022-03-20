using log4net;

using NHibernateQueryViewer.Core;

using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NHibernateQueryViewer
{
    public class MainViewModel : ViewModel
    {
        private readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);
        private LaanQueryFormatter _formatter;
        private QueryParameterEmbedder _parameterEmbedder;

        public ObservableCollection<QueryModel> Queries { get; set; }
        public QueryModel? SelectedQuery { get; set; }

        public ViewOption ViewOption { get; set; } = ViewOption.Format;

        public MainViewModel()
        {
            // TODO: use dependency injection
            _formatter = new LaanQueryFormatter();
            _parameterEmbedder = new QueryParameterEmbedder();
            Queries = new ObservableCollection<QueryModel>();

            PropertyChanged += UpdateViewOptionForSelectedQuery;

            ListenToConnections();
        }

        private async Task ListenToConnections()
        {
            await ListenToUdpConnectionsOn(port: 61234);
        }

        private async Task ListenToUdpConnectionsOn(int port)
        {
            try
            {
                var client = new UdpClient(port);
                while (true)
                {
                    var result = await client.ReceiveAsync();
                    var loggingEvent = Encoding.Unicode.GetString(result.Buffer).Trim();

                    var query = new QueryModel { RawQuery = loggingEvent };

                    Queries.Add(query);
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception);
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
                        SelectedQuery.DisplayQuery = _parameterEmbedder.Embed(SelectedQuery.RawQuery);
                        break;
                    case ViewOption.Format:
                        var query = _parameterEmbedder.Embed(SelectedQuery.RawQuery);
                        SelectedQuery.DisplayQuery = _formatter.Format(query);
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

        public void SetSelectionToFirstQuery()
        {
            SelectedQuery = Queries?.FirstOrDefault();
        }
    }
}
