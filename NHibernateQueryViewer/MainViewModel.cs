using NHibernateQueryViewer.Core;

using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace NHibernateQueryViewer
{
    public class MainViewModel : ViewModel
    {
        public ObservableCollection<QueryModel> Queries { get; set; }
        public QueryModel SelectedQuery { get; set; }
        public string Output { get; set; } = string.Empty;

        public MainViewModel()
        {
            Queries = new ObservableCollection<QueryModel>();
            var args = Environment.GetCommandLineArgs();
            if (args.Length == 2)
            {
                var queryLog = args[1];
                ReadQueryLog(queryLog);
            }
        }

        public void SetSelectionToFirstQuery()
        {
            SelectedQuery = Queries.First();
        }

        private void ReadQueryLog(string queryLog)
        {
            // TODO: refactor code: use dependency injection
            var parser = new QueryParser();
            foreach (string line in File.ReadLines(queryLog))
            {
                var query = parser.Parse(line);
                Queries.Add(query);
            }
        }
    }
}
