namespace NHibernateQueryViewer.Core.Queries;

using System;

public class ConnectionException : Exception
{
    public ConnectionException()
    {
    }

    public ConnectionException(string? message)
        : base(message)
    {
    }

    public ConnectionException(string message, Exception exception)
        : base(message, exception)
    {
    }
}
