using System.Threading.Tasks;

namespace NHibernateQueryViewer.Core
{
    public interface IQueryConnection
    {
        Task<QueryModel> ReceiveQueryAsync();
    }
}
