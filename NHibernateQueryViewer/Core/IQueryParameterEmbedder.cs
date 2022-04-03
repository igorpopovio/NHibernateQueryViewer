namespace NHibernateQueryViewer.Core;

public interface IQueryParameterEmbedder
{
    string Embed(string queryWithParameters);
}
