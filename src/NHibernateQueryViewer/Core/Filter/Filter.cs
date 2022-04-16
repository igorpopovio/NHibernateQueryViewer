namespace NHibernateQueryViewer.Core.Filters;

using System;
using System.Collections.Generic;

public class Filter
{
    public static readonly Filter Empty = new (string.Empty, string.Empty);

    public static readonly string[] Separators = new[] { Environment.NewLine };

    private List<string> _includeKeywords;
    private List<string> _excludeKeywords;

    public Filter(string includeKeywordsText, string excludeKeywordsText)
    {
        IncludeKeywordsText = includeKeywordsText ?? throw new ArgumentNullException(nameof(includeKeywordsText));
        _includeKeywords = new List<string>();
        _includeKeywords.AddRange(includeKeywordsText.Split(Separators, StringSplitOptions.RemoveEmptyEntries));

        ExcludeKeywordsText = excludeKeywordsText ?? throw new ArgumentNullException(nameof(excludeKeywordsText));
        _excludeKeywords = new List<string>();
        _excludeKeywords.AddRange(excludeKeywordsText.Split(Separators, StringSplitOptions.RemoveEmptyEntries));
    }

    public string IncludeKeywordsText { get; private set; } = string.Empty;

    public string ExcludeKeywordsText { get; private set; } = string.Empty;

    public IReadOnlyCollection<string> IncludeKeywords => _includeKeywords;

    public IReadOnlyCollection<string> ExcludeKeywords => _excludeKeywords;
}
