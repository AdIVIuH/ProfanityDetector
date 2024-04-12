/*
MIT License
Copyright (c) 2019
Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:
The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.
THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

using NUnit.Framework;

namespace ProfanityFilter.Tests.Unit;

[TestFixture]
public class HasAnyProfanitiesTests : BaseTest
{
    [TestCase("fluffy")]
    [TestCase("")]
    [TestCase(null)]
    public void HasAnyProfanities_ReturnsFalse_ForNonSwearWord(string nonSwearWord)
    {
        var filter = CreateProfanityFilter();
        Assert.IsFalse(filter.HasAnyProfanities(nonSwearWord));
    }

    [TestCase("shitty")]
    [TestCase("лох")]
    [TestCase("Лох")]
    [TestCase("лоХ")]
    public void HasAnyProfanities_ReturnsFalse_ForWordOnTheAllowList(string word)
    {
        var filter = CreateProfanityFilter();
        Assert.IsTrue(filter.HasAnyProfanities(word));

        filter.AllowList.Add(word);

        Assert.IsFalse(filter.HasAnyProfanities(word));
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase(" ")]
    [TestCase("  ")]
    public void HasAnyProfanities_ReturnsFalse_IfNullOrEmptyInputString(string input)
    {
        var filter = CreateProfanityFilter();
        var result = filter.HasAnyProfanities(input);

        Assert.IsFalse(result);
    }

    [Test]
    public void HasAnyProfanities_ReturnsFalse_WhenMultipleProfanitiesExistAndAreAllowed()
    {
        var filter = CreateProfanityFilter();
        filter.AllowList.Add("cunt");
        filter.AllowList.Add("arse");

        var result = filter.HasAnyProfanities("Scuntarsehorpe");

        Assert.IsFalse(result);
    }

    [Test]
    public void HasAnyProfanities_ReturnsFalse_WhenProfanityDoesNotExist()
    {
        var filter = CreateProfanityFilter();
        var result = filter.HasAnyProfanities("Ireland");

        Assert.IsFalse(result);
    }

    [Test]
    public void HasAnyProfanities_ReturnsFalse_WhenProfanityAddedAsWord_AndInAllowList()
    {
        var filter = new ProfanityFilter();
        const string profanity = "test";

        filter.AddProfanityWord(profanity);
        Assert.IsTrue(filter.HasAnyProfanities(profanity));

        filter.AllowList.Add(profanity);
        Assert.IsFalse(filter.HasAnyProfanities(profanity));
    }

    [Test]
    public void HasAnyProfanities_ReturnsFalse_WhenProfanityAddedAsPattern_AndInAllowList()
    {
        var filter = new ProfanityFilter();
        const string profanity = "test";

        filter.AddProfanityPattern(profanity);
        Assert.IsTrue(filter.HasAnyProfanities(profanity));

        filter.AllowList.Add(profanity);
        Assert.IsFalse(filter.HasAnyProfanities(profanity));
    }

    [Test]
    public void HasAnyProfanities_ReturnsFalse_WhenProfanityIsAaa()
    {
        var filter = CreateProfanityFilter();
        var result = filter.HasAnyProfanities("aaa");

        Assert.IsFalse(result);
    }

    [TestCase("arsehole")]
    [TestCase("shitty")]
    public void HasAnyProfanities_ReturnsTrue_ForSwearWord(string swearWord)
    {
        var filter = CreateProfanityFilter();
        Assert.IsTrue(filter.HasAnyProfanities(swearWord));
    }

    [Test]
    public void HasAnyProfanities_ReturnsTrue_WhenProfanityExists()
    {
        var filter = CreateProfanityFilter();
        var result = filter.HasAnyProfanities("Scunthorpe");

        Assert.IsTrue(result);
    }

    [TestCase("Scuntarsefuckhorpe")]
    [TestCase("fuckлох")]
    [TestCase("fuckingfuck")]
    public void HasAnyProfanities_ReturnsTrue_WhenMultipleProfanitiesExist_InWordsList(string input)
    {
        var filter = CreateProfanityFilter();
        var result = filter.HasAnyProfanities(input);

        Assert.IsTrue(result);
    }

    [TestCase("лохfuck")]
    [TestCase("ПРЕФИКСлохfuck")]
    [TestCase("ПРЕФИКСлохfuckПОСТФИКС")]
    [TestCase("лохfuckПОСТФИКС")]
    public void HasAnyProfanities_ReturnsTrue_WhenMultipleProfanitiesExist_InPatterns(string input)
    {
        var filter = CreateProfanityFilter();
        var result = filter.HasAnyProfanities(input);

        Assert.IsTrue(result);
    }

    [TestCase("лохfuck")]
    [TestCase("ПРЕФИКСлохfuck")]
    [TestCase("ПРЕФИКСлохfuckПОСТФИКС")]
    [TestCase("лохfuckПОСТФИКС")]
    public void HasAnyProfanities_ReturnsTrue_WhenMultipleProfanitiesExist_InWordsOrPatterns(string input)
    {
        var filter = CreateProfanityFilter();
        var result = filter.HasAnyProfanities(input);

        Assert.IsTrue(result);
    }

    [Test]
    public void HasAnyProfanities_ReturnsTrue_WhenProfanityIsADollarDollar()
    {
        var filter = CreateProfanityFilter();
        var result = filter.HasAnyProfanities("a$$");

        Assert.IsTrue(result);
    }

    [TestCase("love is ❤️")]
    public void HasAnyProfanities_ReturnsFalse_WhenInputIsCorrectEmoji(string input)
    {
        var filter = CreateProfanityFilter();
        var result = filter.HasAnyProfanities(input);

        Assert.IsFalse(result, $"Found profanity in the input string '{input}'");
    }

    [TestCase("🖕")]
    public void HasAnyProfanities_ReturnsTrue_WhenInputIsOnlyEmoji(string input)
    {
        var filter = CreateProfanityFilter();
        var result = filter.HasAnyProfanities(input);

        Assert.IsTrue(result, $"Couldn't find any profanity in the input string '{input}'");
    }

    [TestCase("👉👌 гараж")]
    [TestCase("\ud83d\udc49\ud83d\udc4c")]
    [TestCase("👌👈")]
    public void HasAnyProfanities_ReturnsTrue_WhenInputIsComplexEmoji(string input)
    {
        var filter = CreateProfanityFilter();
        var result = filter.HasAnyProfanities(input);

        Assert.IsTrue(result, $"Couldn't find any profanity in the input string '{input}'");
    }
}