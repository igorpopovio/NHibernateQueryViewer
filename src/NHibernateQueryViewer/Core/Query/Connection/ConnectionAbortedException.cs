namespace NHibernateQueryViewer.Core.Queries;

using System;

public class ConnectionAbortedException : ConnectionException
{
    public ConnectionAbortedException()
    {
    }

    public ConnectionAbortedException(string message)
        : base(message)
    {
    }

    public ConnectionAbortedException(string message, Exception exception)
        : base(message, exception)
    {
    }
}
