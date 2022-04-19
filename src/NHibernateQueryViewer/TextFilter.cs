namespace NHibernateQueryViewer;

using NHibernateQueryViewer.Core.Filters;

using System;
using System.Linq;

public class TextFilter : ITextFilter
{
    public bool Filter(string text, string mainFilter, Filter advancedFilter)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return false;
        }

        if (mainFilter is null)
        {
            throw new ArgumentNullException(nameof(mainFilter));
        }

        if (advancedFilter is null)
        {
            throw new ArgumentNullException(nameof(advancedFilter));
        }

        text = text.ToUpperInvariant();
        mainFilter = mainFilter.ToUpperInvariant();

        if (!ShouldAllowTextFromAdvancedFilter(text, advancedFilter))
        {
            return false;
        }

        return ShouldAllowTextFromMainFilter(text, mainFilter);
    }

    private bool ShouldAllowTextFromMainFilter(string text, string mainFilter)
    {
        if (string.IsNullOrWhiteSpace(mainFilter))
        {
            return true;
        }

        return text.Contains(mainFilter, StringComparison.Ordinal);
    }

    private bool ShouldAllowTextFromAdvancedFilter(string text, Filter advancedFilter)
    {
        if (advancedFilter == Core.Filters.Filter.Empty)
        {
            return true;
        }

        foreach (var includeKeyword in advancedFilter.IncludeKeywords)
        {
            var keyword = includeKeyword.ToUpperInvariant();
            if (text.Contains(keyword, StringComparison.Ordinal))
            {
                return true;
            }
        }

        if (!advancedFilter.ExcludeKeywords.Any())
        {
            return false;
        }

        foreach (var excludeKeyword in advancedFilter.ExcludeKeywords)
        {
            var keyword = excludeKeyword.ToUpperInvariant();
            if (text.Contains(keyword, StringComparison.Ordinal))
            {
                return false;
            }
        }

        return true;
    }
}
