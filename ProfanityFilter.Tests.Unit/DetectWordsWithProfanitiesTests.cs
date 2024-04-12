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

using System.Linq;
using FluentAssertions;
using NUnit.Framework;

namespace ProfanityFilter.Tests.Unit;

[TestFixture]
public class DetectWordsWithProfanitiesTests : BaseTest
{
    [Test]
    public void Constructor_SetsAllowList()
    {
        var filter = new ProfanityFilter();
        Assert.IsNotNull(filter.AllowList);
    }

    [TestCase("")]
    [TestCase(null)]
    public void DetectWordsWithProfanities_ReturnsEmptyList_ForEmptyInput(string input)
    {
        var filter = CreateProfanityFilter();
        var profanities = filter.DetectWordsWithProfanities(input);

        Assert.AreEqual(0, profanities.Count);
    }

    [TestCase("You are a complete twat and a dick.", "twat", "dick")]
    [TestCase("You are, a complete twat, and a @dick:", "twat", "dick")]
    [TestCase("You are a complete tWat and a DiCk.", "tWat", "DiCk")]
    public void DetectWordsWithProfanities_Returns2SwearWords(string input, params string[] expected)
    {
        var filter = CreateProfanityFilter();
        var profanities = filter.DetectWordsWithProfanities(input);

        Assert.AreEqual(expected.Length, profanities.Count);
        Assert.IsFalse(expected.Except(profanities).Any());
    }

    [TestCase("2 girls 1 cup and son of a bitch", "2 girls 1 cup", "son of a bitch")]
    [TestCase("2 girls 1 cup is my favourite video", "2 girls 1 cup")]
    [TestCase("2 girls 1 cup is my favourite twatting video", "2 girls 1 cup", "twatting")]
    public void DetectWordsWithProfanities_ReturnsSwearPhrases(string input, params string[] expected)
    {
        var filter = CreateProfanityFilter();
        var swearList = filter.DetectWordsWithProfanities(input, removePartialMatches: true);

        swearList.Count.Should().Be(expected.Length);
        swearList.Except(expected).Should().BeEmpty();
    }

    [Test]
    public void DetectWordsWithProfanities_Returns2SwearPhrase_BecauseOfMatchDeduplication()
    {
        var filter = CreateProfanityFilter();
        var swearList = filter.DetectWordsWithProfanities("2 girls 1 cup is my favourite twatting video", true);

        Assert.AreEqual(2, swearList.Count);
        Assert.AreEqual("2 girls 1 cup", swearList[0]);
        Assert.AreEqual("twatting", swearList[1]);
    }

    [Test]
    public void DetectWordsWithProfanities_Scunthorpe_WithoutAllowList()
    {
        var filter = CreateProfanityFilter();

        var profanities = filter.DetectWordsWithProfanities(
            sentence:
            "I fucking live in Scunthorpe and it is a shit place to live. I would much rather live in penistone you great big cock fuck.",
            removePartialMatches: false);

        profanities.Should().HaveCount(4);
        profanities.Should().Contain(new[] { "fucking", "cock", "fuck", "shit" });
    }

    [Test]
    public void DetectWordsWithProfanities_Scunthorpe_WithAllowList()
    {
        var filter = CreateProfanityFilter();
        filter.AllowList.Add("scunthorpe");
        filter.AllowList.Add("penistone");

        var profanities = filter.DetectWordsWithProfanities(
            sentence:
            "I fucking live in Scunthorpe and it is a shit place to live. I would much rather live in penistone you great big cock fuck.",
            removePartialMatches: false);

        profanities.Should().HaveCount(4);
        profanities.Should().Contain(new[] { "fucking", "cock", "fuck", "shit" });
    }

    [Test]
    public void DetectWordsWithProfanities_Scunthorpe_WithDuplicatesTurnedOff()
    {
        var filter = CreateProfanityFilter();
        filter.AllowList.Add("scunthorpe");
        filter.AllowList.Add("penistone");

        var profanities = filter.DetectWordsWithProfanities(
            sentence:
            "I fucking live in Scunthorpe and it is a shit place to live. I would much rather live in penistone you great big cock fuck.",
            removePartialMatches: true);

        profanities.Should().HaveCount(3);
        profanities.Should().Contain(new[] { "fucking", "cock", "shit" });
    }

    [Test]
    public void DetectWordsWithProfanities_BlocksAllowList()
    {
        var filter = CreateProfanityFilter();
        filter.AllowList.Add("tit");

        var swearList = filter.DetectWordsWithProfanities("You are a complete twat and a total tit.", true);

        Assert.AreEqual(1, swearList.Count);
        Assert.AreEqual("twat", swearList[0]);
    }

    [Test]
    public void DetectWordsWithProfanities_Scunthorpe_WithDuplicatesTurnedOffAndNoAllowList()
    {
        var filter = CreateProfanityFilter();

        var profanities = filter.DetectWordsWithProfanities(
            "I fucking live in Scunthorpe and it is a shit place to live. I would much rather live in penistone you great big cock fuck.",
            true);

        profanities.Should().HaveCount(3);
        profanities.Should().Contain(new[] { "fucking", "cock", "shit" });
    }

    [Test]
    public void DetectWordsWithProfanities_MultipleScunthorpes()
    {
        var filter = CreateProfanityFilter();
        var profanities = filter.DetectWordsWithProfanities("Scunthorpe Scunthorpe", true);

        Assert.AreEqual(0, profanities.Count);
    }

    [Test]
    public void DetectWordsWithProfanities_MultipleScunthorpes_SingleCunt()
    {
        var filter = CreateProfanityFilter();

        var swearList = filter.DetectWordsWithProfanities("Scunthorpe cunt Scunthorpe");

        Assert.AreEqual(1, swearList.Count);
        Assert.AreEqual("cunt", swearList[0]);
    }

    [Test]
    public void DetectWordsWithProfanities_MultipleScunthorpesMultiCunt()
    {
        var filter = CreateProfanityFilter();

        var swearList = filter.DetectWordsWithProfanities("Scunthorpe cunt Scunthorpe cunt");

        Assert.AreEqual(1, swearList.Count);
        Assert.AreEqual("cunt", swearList[0]);
    }

    [Test]
    public void DetectWordsWithProfanities_ScunthorpePenistone()
    {
        var filter = CreateProfanityFilter();

        var swearList = filter.DetectWordsWithProfanities("ScUnThOrPePeNiStOnE", true);

        Assert.AreEqual(0, swearList.Count);
    }

    [Test]
    public void DetectWordsWithProfanities_ScunthorpePenistone_OneKnob()
    {
        var filter = CreateProfanityFilter();

        var swearList = filter.DetectWordsWithProfanities("ScUnThOrPePeNiStOnE KnOb", true);

        Assert.AreEqual(1, swearList.Count);
        Assert.AreEqual("KnOb", swearList[0]);
    }

    [Test]
    public void DetectWordsWithProfanities_LongerSentence()
    {
        var filter = CreateProfanityFilter();

        var swearList =
            filter.DetectWordsWithProfanities(
                "You are a stupid little twat, and you like to blow your load in an alaskan pipeline.", true);

        Assert.AreEqual(4, swearList.Count);
        Assert.AreEqual("alaskan pipeline", swearList[0]);
        Assert.AreEqual("blow your load", swearList[1]);
        Assert.AreEqual("stupid", swearList[2]);
        Assert.AreEqual("twat", swearList[3]);
    }

    [TestCase("cock")]
    public void DetectWordsWithProfanities_ForSingleWord(string word)
    {
        var filter = CreateProfanityFilter();

        var swearList = filter.DetectWordsWithProfanities(word, true);

        Assert.AreEqual(1, swearList.Count);
        Assert.AreEqual(word, swearList[0]);
    }
}