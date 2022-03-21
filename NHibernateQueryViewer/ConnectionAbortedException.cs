using System;

namespace NHibernateQueryViewer
{
    public class ConnectionAbortedException : ConnectionException
    {
        public ConnectionAbortedException(string message, Exception exception)
            : base(message, exception) { }
    }
}
