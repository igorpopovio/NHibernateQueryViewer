using System;

namespace NHibernateQueryViewer
{
    public class QueryModel
    {
        public DateTime DateTime { get; set; }
        public string Parameterized { get; set; } = string.Empty;
        public string WithParameters { get; set; } = string.Empty;
    }
}
