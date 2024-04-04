using System.Collections.Generic;

namespace ProfanityFilter.Models;

internal record CensorProfanityResult(
    string CensoredSentence,
    IReadOnlyList<string> AppliedProfanities
);