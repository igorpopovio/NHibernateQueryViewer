namespace NHibernateQueryViewer;

using NHibernateQueryViewer.Core.Filters;
using NHibernateQueryViewer.Core.Queries;

using Stylet;

using StyletIoC;

using System;

public class Bootstrapper : Bootstrapper<MainViewModel>
{
    protected override void ConfigureIoC(IStyletIoCBuilder builder)
    {
        if (builder == null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        base.ConfigureIoC(builder);

        builder.Bind<IQueryFormatter>().To<LaanQueryFormatter>().InSingletonScope();
        builder.Bind<IQueryParameterEmbedder>().To<QueryParameterEmbedder>().InSingletonScope();
        builder.Bind<IQueryConnection>().To<QueryConnection>();
        builder.Bind<IFilterIO>().To<FilterIO>().InSingletonScope();
    }
}
