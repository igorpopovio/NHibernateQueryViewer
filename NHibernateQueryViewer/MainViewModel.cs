using System.ComponentModel;
using System.Text;

namespace NHibernateQueryViewer
{
    public class MainViewModel : ViewModel
    {
        public string Input { get; set; } = string.Empty;
        public string Output { get; set; } = string.Empty;

        public MainViewModel()
        {
            PropertyChanged += EmbedQueryParameters;
        }

        private void EmbedQueryParameters(object? sender, PropertyChangedEventArgs args)
        {
            if (args.PropertyName != nameof(Input)) return;

            try
            {
                var query = new QueryParser().Parse(Input);
                Output = query.WithParameters;
            }
            catch
            {
                var message = new StringBuilder();
                message.AppendLine("Error. Please enter a query like the following:");
                message.AppendLine("SELECT Id FROM Person WHERE Id = @p1;@p1 = 1 [Type: Int32 (0,0,0)];");
                Output = message.ToString();
            }
        }
    }
}
