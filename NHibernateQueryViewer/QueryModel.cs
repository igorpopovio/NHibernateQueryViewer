using NHibernateQueryViewer.Core;

using System;

namespace NHibernateQueryViewer
{
    public class QueryModel : ObservableObject
    {
        public DateTime DateTime { get; set; }
        public string Parameterized { get; set; } = string.Empty;
        public string WithParameters { get; set; } = string.Empty;
    }
}
