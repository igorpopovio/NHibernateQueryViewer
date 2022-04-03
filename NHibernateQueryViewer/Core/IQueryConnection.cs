namespace NHibernateQueryViewer.Core;

using System.Threading.Tasks;

public interface IQueryConnection
{
    Task<QueryModel> ReceiveQueryAsync();
}
