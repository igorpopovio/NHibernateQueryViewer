namespace NHibernateQueryViewer;

using System;

public class ConnectionException : Exception
{
    public ConnectionException(string message, Exception exception)
        : base(message, exception)
    {
    }
}
