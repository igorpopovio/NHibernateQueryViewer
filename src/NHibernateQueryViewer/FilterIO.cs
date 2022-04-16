namespace NHibernateQueryViewer;

using NHibernateQueryViewer.Core.Filters;

using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

public class FilterIO : IFilterIO
{
    private const string ApplicationName = "NHibernateQueryViewer";
    private const string IncludeFilterFileName = "IncludeFilter.txt";
    private const string ExcludeFilterFileName = "ExcludeFilter.txt";

    private readonly string _applicationDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
    private readonly Encoding _defaultEncoding = Encoding.UTF8;

    public event EventHandler<FilterSavedEventArgs>? Saved;

    public async Task<Filter> Load()
    {
        if (!File.Exists(GetPath(IncludeFilterFileName)))
        {
            return Filter.Empty;
        }

        if (!File.Exists(GetPath(ExcludeFilterFileName)))
        {
            return Filter.Empty;
        }

        var includeKeywordsText = await File.ReadAllTextAsync(GetPath(IncludeFilterFileName), _defaultEncoding);
        var excludeKeywordsText = await File.ReadAllTextAsync(GetPath(ExcludeFilterFileName), _defaultEncoding);

        return new Filter(includeKeywordsText, excludeKeywordsText);
    }

    public async Task Save(Filter filter)
    {
        if (filter is null)
        {
            throw new ArgumentNullException(nameof(filter));
        }

        Directory.CreateDirectory(GetApplicationDataPath());

        await File.WriteAllTextAsync(GetPath(IncludeFilterFileName), filter.IncludeKeywordsText, _defaultEncoding);
        await File.WriteAllTextAsync(GetPath(ExcludeFilterFileName), filter.ExcludeKeywordsText, _defaultEncoding);

        Saved?.Invoke(this, new FilterSavedEventArgs { Filter = filter });
    }

    private string GetPath(string fileName) => Path.Combine(GetApplicationDataPath(), fileName);

    private string GetApplicationDataPath() => Path.Combine(_applicationDataPath, ApplicationName);
}
