namespace NHibernateQueryViewer;

using NHibernateQueryViewer.Core;

using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

public class QueryConnection : IQueryConnection, IDisposable
{
    public const int DefaultPort = 61234;
    private UdpClient _udpClient;
    private bool _disposed;

    private ISet<SocketError> _abortedErrorCodes = new HashSet<SocketError>()
    {
        SocketError.OperationAborted,
        SocketError.ConnectionAborted,
    };

    public QueryConnection()
        : this(new UdpClient(DefaultPort))
    {
    }

    public QueryConnection(UdpClient udpClient)
    {
        _udpClient = udpClient;
    }

    public async Task<QueryModel> ReceiveQueryAsync()
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(QueryConnection));
        }

        try
        {
            // TODO: call ReceiveAsync(CancellationToken cancellationToken)
            // and implement proper cancellation instead of checking error codes
            var result = await _udpClient.ReceiveAsync();
            var loggingEvent = Encoding.UTF8.GetString(result.Buffer).Trim();
            return new QueryModel { RawQuery = loggingEvent };
        }
        catch (SocketException exception)
        {
            if (_abortedErrorCodes.Contains(exception.SocketErrorCode))
            {
                throw new ConnectionAbortedException("Connection was aborted", exception);
            }
            else
            {
                throw new ConnectionException("Unknown connection error", exception);
            }
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _udpClient.Dispose();
            }

            _disposed = true;
        }
    }
}
