using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;

namespace ProfanityFilter.Benchmark;

[MemoryDiagnoser]
[RankColumn]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
public class ProfanityFilterBenchmarks
{
    private ProfanityFilter _profanityFilter;

    [GlobalSetup]
    public void Setup()
    {
        _profanityFilter = new ProfanityFilter();
        _profanityFilter.AddProfanityWords(ProfanitiesDictionary.Words);
        _profanityFilter.AddProfanityPatterns(ProfanitiesDictionary.Patterns);
    }

    private const string LongClientName = @"Ебаный рот этого казино, блять. Ты кто такой, сука? Чтоб это сделать?
Вы че, дибилы? Вы че, ебанутые? Вы внатуре ебанутые.
Эта сидит там, чешет колоду, блять. Этот стоит, говорит: 'Я тебе щас тоже раздам'
Еб твою мать, у вас дилер есть, чтоб это делать на моих глазах, мудак ебаный! Дегенерат ебучий!
Вот пока ты это делал, дибил ебаный, сука, блять, так все и происходило!
Блять, вы че, действительно идиоты, а? Блять, дифиченты какие-то, ебаный ваш рот, а. Ты че делаешь?!
ЕБАНЫЙ ТВОЙ РОТ, КАКОГО ХУЯ ОНИ В ДРУГОМ ПОРЯДКЕ РАЗЛОЖЕНЫ, ТЫ РАСПЕЧАТАЛА КОЛОДУ НА МОИХ ГЛАЗАХ, БЛЯТЬ!
КАК ОНИ МОГУТ БЫТЬ ТАМ РАЗЛОЖЕНЫ В ДРУГОМ ПОРЯДКЕ? ЕБАНЫЙ ТВОЙ РОТ, БЛЯТЬ, ВЫ ЧЕ, В КИОСКАХ ИХ ЗАРЯЖАЕТЕ? СУКА ЕБАНАЯ, ПАДЛА БЛЯДСКАЯ!";

    private const string ShortClientName = "Ебанное казино";
    //
    // [Benchmark]
    // public string CensorString_WithShortString()
    // {
    //     return _profanityFilter.CensorString(ShortClientName);
    // }

    [Benchmark]
    public string CensorString_WithLongString()
    {
        return _profanityFilter.CensorString(LongClientName);
    }

    // [Benchmark]
    // public IReadOnlyList<string> DetectWordsWithProfanities_WithShortString()
    // {
    //     return _profanityFilter.DetectWordsWithProfanities(ShortClientName);
    // }

    [Benchmark]
    public IReadOnlyList<string> DetectWordsWithProfanities_WithLongString()
    {
        return _profanityFilter.DetectWordsWithProfanities(LongClientName);
    }

    [Benchmark]
    public void GetNormalizedInputOrCache_WithLongString()
    {
        _profanityFilter.GetNormalizedInputOrCache(LongClientName);
    }

    [Benchmark]
    public void NormalizeInput_WithLongString()
    {
        _profanityFilter.NormalizeInput(LongClientName);
    }

    [Benchmark]
    public void GetMatchedProfanities_WithLongString()
    {
        _profanityFilter.GetMatchedProfanities(
            input: LongClientName,
            includePartialMatch: true,
            includePatterns: true);
    }

    [Benchmark]
    public void GetMatchedProfanities_WithLongString2()
    {
        _profanityFilter.GetMatchedProfanities(
            input: LongClientName,
            includePartialMatch: false,
            includePatterns: true);
    }

    [Benchmark]
    public void GetMatchedProfanities_WithLongString3()
    {
        _profanityFilter.GetMatchedProfanities(
            input: LongClientName,
            includePartialMatch: false,
            includePatterns: false);
    }

    [Benchmark]
    public void GetMatchedProfanities_WithLongString4()
    {
        _profanityFilter.GetMatchedProfanities(
            input: LongClientName,
            includePartialMatch: true,
            includePatterns: false);
    }
}