namespace ProfanityFilter.Models;

public record CompleteWord(
    int StartWordIndex,
    int EndWordIndex,
    string WholeWord
);