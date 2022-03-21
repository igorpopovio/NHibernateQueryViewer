using NHibernateQueryViewer.Core;

using Stylet;

using StyletIoC;

namespace NHibernateQueryViewer
{
    public class Bootstrapper : Bootstrapper<MainViewModel>
    {
        protected override void ConfigureIoC(IStyletIoCBuilder builder)
        {
            base.ConfigureIoC(builder);

            builder.Bind<IQueryFormatter>().To<LaanQueryFormatter>().InSingletonScope();
            builder.Bind<IQueryParameterEmbedder>().To<QueryParameterEmbedder>().InSingletonScope();
            builder.Bind<IQueryConnection>().To<QueryConnection>();
        }
    }
}
