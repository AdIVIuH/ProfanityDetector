#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using ProfanityFilter.Extensions;

namespace ProfanityFilter.Models;

internal class Text
{
    private readonly string _originalString;
    private readonly HashSet<string> _determinedProfanities = new();
    private readonly HashSet<string> _usedSwearWords = new();
    private readonly Lazy<IReadOnlyList<WordInSentence>> _words;
    private readonly Lazy<string> _normalizedString;

    public Text(string originalString)
    {
        _originalString = originalString;
        _words = new Lazy<IReadOnlyList<WordInSentence>>(ExtractWords);
        _normalizedString = new Lazy<string>(Normalize);
    }

    // ReSharper disable once ConvertToAutoProperty
    public string OriginalString => _originalString;

    public string NormalizedString => _normalizedString.Value; // закэшированная проекция

    public IReadOnlyList<string> DeterminedProfanities => _determinedProfanities.ToList().AsReadOnly();
    public IReadOnlyList<string> UsedSwearWords => _usedSwearWords.ToList().AsReadOnly();
    public IReadOnlyList<WordInSentence> Words => _words.Value;

    public void AddProfanity(string profanity)
    {
        ArgumentNullException.ThrowIfNull(profanity);
        _determinedProfanities.Add(profanity);
    }

    public bool RemoveProfanity(string profanity)
    {
        ArgumentNullException.ThrowIfNull(profanity);
        return _determinedProfanities.Remove(profanity);
    }

    private IReadOnlyList<WordInSentence> ExtractWords() => OriginalString.ExtractWords()
        .Select(x => new WordInSentence(
            startIndex: x.StartWordIndex,
            originalWord: x.WholeWord
        ))
        .ToList();

    private string Normalize()
    {
        // TODO not implemented yet add gomogliths }|{ -> ж
        var resultWords = Words
            .Select(cw => cw.NormalizedWord);
        var joinedWords = string.Join(' ', resultWords);

        return joinedWords;
    }
}