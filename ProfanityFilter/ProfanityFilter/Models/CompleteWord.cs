namespace ProfanityFilter.Models;

internal record CompleteWord(
    int StartWordIndex,
    int EndWordIndex,
    string WholeWord
);