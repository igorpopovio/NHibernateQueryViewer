using log4net;

using NHibernateQueryViewer.Core;

using System;
using System.Collections.ObjectModel;
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

        public ObservableCollection<QueryModel>? Queries { get; set; }
        public QueryModel? SelectedQuery { get; set; }

        public ViewOption ViewOption { get; set; } = ViewOption.Format;

        public MainViewModel()
        {
            Initialize();
        }

        private async Task Initialize()
        {
            Queries = new ObservableCollection<QueryModel>();
            await ListenToUdpConnectionsOn(port: 61234);
        }

        private async Task ListenToUdpConnectionsOn(int port)
        {
            var parser = new QueryParser();

            try
            {
                var client = new UdpClient(port);
                while (true)
                {
                    var result = await client.ReceiveAsync();
                    var loggingEvent = Encoding.Unicode.GetString(result.Buffer);
                    var query = parser.Parse(loggingEvent);
                    Queries.Add(query);
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception);
            }
        }

        public void SetSelectionToFirstQuery()
        {
            SelectedQuery = Queries?.FirstOrDefault();
        }
    }
}
