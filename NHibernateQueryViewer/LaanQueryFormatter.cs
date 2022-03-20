using Laan.Sql.Formatter;

using NHibernateQueryViewer.Core;

namespace NHibernateQueryViewer
{
    public class LaanQueryFormatter : IQueryFormatter
    {
        private FormattingEngine _formatter;

        public LaanQueryFormatter()
        {
            _formatter = new FormattingEngine();
        }

        public string Format(string query)
        {
            return _formatter.Execute(query);
        }
    }
}
