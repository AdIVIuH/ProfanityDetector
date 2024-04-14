using System;
using System.Collections.Generic;
using System.Linq;

namespace ProfanityFilter.Models;

public class TextWithProfanities
{
    private readonly HashSet<string> _profanities;

    private TextWithProfanities(string text, IEnumerable<string> profanities)
    {
        Text = text;
        _profanities = new HashSet<string>(profanities);
    }

    public static TextWithProfanities Create(string text, IEnumerable<string> profanities)
    {
        ArgumentNullException.ThrowIfNull(text);
        ArgumentNullException.ThrowIfNull(profanities);
        if (!profanities.Any())
            throw new ArgumentOutOfRangeException(nameof(profanities), $"Argument {profanities} should not be empty");
        return new TextWithProfanities(text, profanities);
    }

    public static TextWithProfanities Create(string text, params string[] profanities) =>
        Create(text, (IEnumerable<string>)profanities);

    public string Text { get; }

    // TODO постоянное перевыделение памяти, хотя по факту нужно только при изменении HashSet'а
    public IReadOnlyList<string> Profanities => _profanities.ToList();

    public void AddProfanity(string profanity)
    {
        ArgumentNullException.ThrowIfNull(profanity);
        _profanities.Add(profanity);
    }

    public bool RemoveProfanity(string profanity)
    {
        ArgumentNullException.ThrowIfNull(profanity);
        return _profanities.Remove(profanity);
    }
}