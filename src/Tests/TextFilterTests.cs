namespace Tests;

using NHibernateQueryViewer;
using NHibernateQueryViewer.Core.Filters;

using NUnit.Framework;

public class TextFilterTests
{
    [Test]
    public void DoesntShowBlankText()
    {
        Assert.That(!new TextFilter().Filter(string.Empty, string.Empty, Filter.Empty));
    }

    [Test]
    public void ShowsWordWhenNotFilteringAnything()
    {
        Assert.That(new TextFilter().Filter("word", string.Empty, Filter.Empty));
    }

    [Test]
    public void ShowsWordWhenFilteringThatWord()
    {
        Assert.That(new TextFilter().Filter("word", "word", Filter.Empty));
    }

    [Test]
    public void ShowsIncludedKeyword()
    {
        var advancedFilter = new Filter("word", string.Empty);
        Assert.That(new TextFilter().Filter("word", string.Empty, advancedFilter));
    }

    [Test]
    public void ShowsWordWhenFilteringByBlanks()
    {
        var advancedFilter = new Filter("     ", string.Empty);
        Assert.That(new TextFilter().Filter("word", string.Empty, advancedFilter));
    }

    [Test]
    public void DoesntShowUnincludedKeyword()
    {
        var advancedFilter = new Filter("word", string.Empty);
        Assert.That(!new TextFilter().Filter("whatever", string.Empty, advancedFilter));
    }

    [Test]
    public void DoesntShowExcludedKeyword()
    {
        var advancedFilter = new Filter(string.Empty, "word");
        Assert.That(!new TextFilter().Filter("word", string.Empty, advancedFilter));
    }

    [Test]
    public void ShowsUnexcludedKeyword()
    {
        var advancedFilter = new Filter(string.Empty, "word");
        Assert.That(new TextFilter().Filter("whatever", string.Empty, advancedFilter));
    }

    [Test]
    public void CombinedFilters()
    {
        var advancedFilter = new Filter("include", "exclude");
        var filter = new TextFilter();

        Assert.Multiple(() =>
        {
            Assert.That(filter.Filter("include", "include", advancedFilter));
            Assert.That(!filter.Filter("include", "filter", advancedFilter));
            Assert.That(filter.Filter("include filter", "filter", advancedFilter));
            Assert.That(filter.Filter("include exclude", string.Empty, advancedFilter));
            Assert.That(filter.Filter("include exclude", "include", advancedFilter));
            Assert.That(!filter.Filter("include exclude", "whatever", advancedFilter));
        });
    }
}
