using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using ProfanityFilter.Extensions;

namespace ProfanityFilter.Tests.Unit;

[TestFixture]
public class StringExtensionsTests
{
    [Test]
    public void ExtractWords_ShouldReturnEmptyEnumeration_ForEmptyString()
    {
        var input = "";
        var result = input.ExtractWords();

        result.Should().BeEmpty();
    }

    [TestCase("первое слово", "первое", "слово")]
    [TestCase("первое  слово", "первое", "слово")]
    public void ExtractWords_ShouldReturn_TwoWords(string input, params string[] expected)
    {
        var result = input.ExtractWords().ToArray();
        
        result.Length.Should().Be(expected.Length);
        result.Should().Contain(expected);
    }
    
    [TestCase("первое12слово", "первое12слово")]
    [TestCase("первое 12 слово", "первое", "12", "слово")]
    [TestCase("12первое слово", "12первое", "слово")]
    [TestCase("первое слово12", "первое", "слово12")]
    public void ExtractWords_ShouldReturnWords_WithNumbers(string input, params string[] expected)
    {
        var result = input.ExtractWords().ToArray();
        
        result.Length.Should().Be(expected.Length);
        result.Should().Contain(expected);
    }

    [TestCase(" первое", "первое")]
    public void ExtractWords_ShouldReturnEmptyEnumeration_(string input, string expected)
    {
        var result = input.ExtractWords();

        result.Should().BeEquivalentTo(expected);
    }

    [TestCase("'слово", "слово")]
    [TestCase("слово'", "слово")]
    [TestCase("'слово'", "слово")]
    [TestCase("\"слово", "слово")]
    [TestCase("слово\"", "слово")]
    [TestCase("\"слово\"", "слово")]
    [TestCase("«слово", "слово")]
    [TestCase("слово»", "слово")]
    [TestCase("«слово»", "слово")]
    [TestCase("`слово", "слово")]
    [TestCase("слово`", "слово")]
    [TestCase("`слово`", "слово")]
    public void ExtractWords_ShouldReturnStringWithoutQuotes(string input, string expected)
    {
        var result = input.ExtractWords();

        result.Should().BeEquivalentTo(expected);
    }

    [TestCase("(слово)", "слово")]
    public void ExtractWords_ShouldReturnStringWithoutBrace(string input, string expected)
    {
        var result = input.ExtractWords();

        result.Should().BeEquivalentTo(expected);
    }

    [TestCase(',')]
    [TestCase('\u2014')] // Em Dash
    [TestCase('\u2013')] // Среднее (En) тире «–»
    [TestCase('\u2012')] // цифровое тире «‒»
    [TestCase('.')]
    [TestCase('!')]
    [TestCase(':')]
    [TestCase('?')]
    [TestCase(';')]
    [TestCase('\\')]
    [TestCase('/')]
    [TestCase('\n')]
    [TestCase('\r')]
    // [TestCase('=')] не разделители, по идее
    // [TestCase('+')] не разделители, по идее
    public void ExtractWords_ShouldReturnTwoWords_ByDifferentSeparators(char separator)
    {
        var input = $"первое{separator}второе";
        var result = input.ExtractWords().ToArray();

        var expectedResult = new[] { "первое", "второе" };
        result.Length.Should().Be(expectedResult.Length);
        result.Should().Contain(expectedResult);
    }
    
    [TestCase("первое_слово", "первое", "слово")]
    [TestCase("первое_", "первое")]
    [TestCase("_первое", "первое")]
    [TestCase("_первое_", "первое")]
    public void ExtractWords_ShouldReturnWords_WithoutUnderlining(string input, params string[] expected)
    {
        var result = input.ExtractWords().ToArray();
        
        result.Length.Should().Be(expected.Length);
        result.Should().Contain(expected);
    }
}