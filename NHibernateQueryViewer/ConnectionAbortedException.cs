namespace NHibernateQueryViewer;

using System;

public class ConnectionAbortedException : ConnectionException
{
    public ConnectionAbortedException(string message, Exception exception)
        : base(message, exception)
    {
    }
}
