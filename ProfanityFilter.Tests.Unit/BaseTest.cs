namespace ProfanityFilter.Tests.Unit;

public abstract class BaseTest
{
    protected ProfanityFilter CreateProfanityFilter()
    {
        var filter = new ProfanityFilter();
        filter.AddProfanityWords(ProfanitiesDictionary.Words);
        filter.AddProfanityPatterns(ProfanitiesDictionary.Patterns);
        return filter;
    }
}