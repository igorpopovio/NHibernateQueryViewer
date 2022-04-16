namespace NHibernateQueryViewer.Core.Queries;

using System.Threading.Tasks;

public interface IQueryConnection
{
    Task<Query> ReceiveQueryAsync();
}
