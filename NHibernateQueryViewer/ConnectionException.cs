using System;

namespace NHibernateQueryViewer
{
    public class ConnectionException : Exception
    {
        public ConnectionException(string message, Exception exception)
            : base(message, exception) { }
    }
}
