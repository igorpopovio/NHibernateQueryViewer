namespace NHibernateQueryViewer.Core.Filters;

using System;
using System.Collections.Generic;
using System.Linq;

public class Filter : IEquatable<Filter>
{
    public static readonly Filter Empty = new (string.Empty, string.Empty);

    public static readonly string[] Separators = new[] { Environment.NewLine };

    private HashSet<string> _includeKeywords;
    private HashSet<string> _excludeKeywords;

    public Filter(string includeKeywordsText, string excludeKeywordsText)
    {
        IncludeKeywordsText = includeKeywordsText ?? throw new ArgumentNullException(nameof(includeKeywordsText));
        _includeKeywords = new HashSet<string>(NormalizeKeywords(includeKeywordsText));

        ExcludeKeywordsText = excludeKeywordsText ?? throw new ArgumentNullException(nameof(excludeKeywordsText));
        _excludeKeywords = new HashSet<string>(NormalizeKeywords(excludeKeywordsText));
    }

    public string IncludeKeywordsText { get; private set; } = string.Empty;

    public string ExcludeKeywordsText { get; private set; } = string.Empty;

    public IReadOnlyCollection<string> IncludeKeywords => _includeKeywords;

    public IReadOnlyCollection<string> ExcludeKeywords => _excludeKeywords;

    public static bool operator ==(Filter filter1, Filter filter2)
    {
        if (((object)filter1) == null || ((object)filter2) == null)
        {
            return object.Equals(filter1, filter2);
        }

        return filter1.Equals(filter2);
    }

    public static bool operator !=(Filter filter1, Filter filter2)
    {
        return !(filter1 == filter2);
    }

    public bool Equals(Filter? other)
    {
        if (ReferenceEquals(null, other))
        {
            return false;
        }

        return
            this.IncludeKeywords.SequenceEqual(other.IncludeKeywords) &&
            this.ExcludeKeywords.SequenceEqual(other.ExcludeKeywords);
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(obj, null))
        {
            return false;
        }

        if (ReferenceEquals(this, obj))
        {
            return true;
        }

        var that = obj as Filter;
        if (ReferenceEquals(that, null))
        {
            return false;
        }

        return this.Equals(that);
    }

    public override int GetHashCode() => HashCode.Combine(IncludeKeywords, ExcludeKeywords);

    private IEnumerable<string> NormalizeKeywords(string text) => text
        .Split(Separators, StringSplitOptions.RemoveEmptyEntries)
        .Select(entry => entry.Trim())
        .Where(entry => !string.IsNullOrWhiteSpace(entry));
}
