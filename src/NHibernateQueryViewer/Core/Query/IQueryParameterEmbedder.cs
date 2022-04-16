namespace NHibernateQueryViewer.Core.Queries;

public interface IQueryParameterEmbedder
{
    string Embed(string queryWithParameters);
}
