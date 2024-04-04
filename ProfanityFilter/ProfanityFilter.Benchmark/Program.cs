using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

BenchmarkRunner.Run<ProfanityFilterBenchmark>();

public class ProfanityFilterBenchmark
{
    [Benchmark]
    public string ShortStringTest()
    {
        var filter = new ProfanityFilter.ProfanityFilter();

        var clientName = "Ебанное казино";

        return filter.CensorString(clientName);
    }

    [Benchmark]
    public string LongStringTest()
    {
        var filter = new ProfanityFilter.ProfanityFilter();

        var clientName = @"Ебаный рот этого казино, блять. Ты кто такой, сука? Чтоб это сделать?
Вы че, дибилы? Вы че, ебанутые? Вы внатуре ебанутые.
Эта сидит там, чешет колоду, блять. Этот стоит, говорит: 'Я тебе щас тоже раздам'
Еб твою мать, у вас дилер есть, чтоб это делать на моих глазах, мудак ебаный! Дегенерат ебучий!
Вот пока ты это делал, дибил ебаный, сука, блять, так все и происходило!
Блять, вы че, действительно идиоты, а? Блять, дифиченты какие-то, ебаный ваш рот, а. Ты че делаешь?!
ЕБАНЫЙ ТВОЙ РОТ, КАКОГО ХУЯ ОНИ В ДРУГОМ ПОРЯДКЕ РАЗЛОЖЕНЫ, ТЫ РАСПЕЧАТАЛА КОЛОДУ НА МОИХ ГЛАЗАХ, БЛЯТЬ!
КАК ОНИ МОГУТ БЫТЬ ТАМ РАЗЛОЖЕНЫ В ДРУГОМ ПОРЯДКЕ? ЕБАНЫЙ ТВОЙ РОТ, БЛЯТЬ, ВЫ ЧЕ, В КИОСКАХ ИХ ЗАРЯЖАЕТЕ? СУКА ЕБАНАЯ, ПАДЛА БЛЯДСКАЯ!";

        return filter.CensorString(clientName);
    }
}