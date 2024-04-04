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

using System;
using FluentAssertions;
using NUnit.Framework;
using Assert = NUnit.Framework.Assert;

namespace ProfanityFilter.Tests.Unit;

[TestFixture]
public class AllowListTests
{
    [Test]
    public void Constructor_SetsAllowList()
    {
        var filter = new ProfanityFilter();
        Assert.IsNotNull(filter.AllowList);
    }

    [Test]
    public void Constructor_SetsAllowListToEmpty()
    {
        var filter = new AllowList();
        Assert.AreEqual(0, filter.Count);
    }

    [Test]
    public void Add_ThrowsArgumentNullException_IfInputStringIsNullOrEmpty()
    {
        var allowList = new AllowList();

        var act = () => allowList.Add("");
        act.Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void Add_InsertsItemIntoTheAllowList()
    {
        var allowList = new AllowList();

        Assert.AreEqual(0, allowList.Count);

        allowList.Add("Scunthorpe");

        Assert.AreEqual(1, allowList.Count);
    }

    [Test]
    public void Add_InsertsLowercaseItemIntoTheAllowList()
    {
        var allowList = new AllowList();

        allowList.Add("Scunthorpe");

        Assert.IsTrue(allowList.Contains("scunthorpe"));
    }

    [Test]
    public void Add_DoesNotAllowDuplicateEntries()
    {
        var allowList = new AllowList();

        Assert.AreEqual(0, allowList.Count);

        allowList.Add("Scunthorpe");

        Assert.AreEqual(1, allowList.Count);

        allowList.Add("Scunthorpe");

        Assert.AreEqual(1, allowList.Count);
    }

    [Test]
    public void Add_DoesntAllowDuplicateEntriesOfMixedCase()
    {
        var allowList = new AllowList();

        Assert.AreEqual(0, allowList.Count);

        allowList.Add("scunthorpe");

        Assert.AreEqual(1, allowList.Count);

        allowList.Add("Scunthorpe");

        Assert.AreEqual(1, allowList.Count);

        allowList.Add("ScunThorpe");

        Assert.AreEqual(1, allowList.Count);
    }

    [Test]
    public void Contains_ThrowsArgumentNullException_IfInputStringIsNullOrEmpty()
    {
        var allowList = new AllowList();

        var act = () => allowList.Contains("");
        act.Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void Contains_ReturnsTrue_ForAllowListItemInTheList()
    {
        var allowList = new AllowList();
        allowList.Add("Scunthorpe");
        allowList.Add("Penistone");

        Assert.IsTrue(allowList.Contains("Scunthorpe"));
        Assert.IsTrue(allowList.Contains("Penistone"));
    }

    [Test]
    public void Contains_ReturnsTrue_ForAllowListItemInTheListWithMixedCase()
    {
        var allowList = new AllowList();
        allowList.Add("Scunthorpe");
        allowList.Add("Penistone");

        Assert.IsTrue(allowList.Contains("ScunThorpe"));
        Assert.IsTrue(allowList.Contains("PeniStone"));
    }

    [Test]
    public void Contains_ReturnsFalse_ForAllowListItemNotInTheList()
    {
        var allowList = new AllowList();
        allowList.Add("Scunthorpe");
        allowList.Add("Penistone");

        Assert.IsFalse(allowList.Contains("Wibble"));
        Assert.IsFalse(allowList.Contains("Gobble"));
    }

    [Test]
    public void Count_ReturnsTwo_ForTwoEntriesInTheList()
    {
        var allowList = new AllowList();
        allowList.Add("Scunthorpe");
        allowList.Add("Penistone");

        Assert.AreEqual(2, allowList.Count);
    }

    [Test]
    public void Count_ReturnsTwo_ForTwoEntriesInTheListAfterMixedcaseAdditions()
    {
        var allowList = new AllowList();
        allowList.Add("Scunthorpe");
        allowList.Add("Penistone");
        allowList.Add("ScunThorpe");
        allowList.Add("PeniStone");

        Assert.AreEqual(2, allowList.Count);
    }

    [Test]
    public void Clear_RemovesEntriesFromTheList()
    {
        var allowList = new AllowList();
        allowList.Add("Scunthorpe");
        allowList.Add("Penistone");

        Assert.AreEqual(2, allowList.Count);

        allowList.Clear();

        Assert.AreEqual(0, allowList.Count);
    }

    [Test]
    public void Remove_ThrowsArgumentNullException_IfInputStringIsNullOrEmpty()
    {
        var allowList = new AllowList();

        var act = () => allowList.Remove("");
        act.Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void Remove_EntryFromTheAllowList()
    {
        var allowList = new AllowList();
        allowList.Add("Scunthorpe");
        allowList.Add("Penistone");

        Assert.AreEqual(2, allowList.Count);

        allowList.Remove("Scunthorpe");

        Assert.AreEqual(1, allowList.Count);
        Assert.IsFalse(allowList.Contains("Scunthorpe"));
        Assert.IsTrue(allowList.Contains("Penistone"));
    }

    [Test]
    public void Remove_MixedCaseEntryFromTheAllowList()
    {
        var allowList = new AllowList();
        allowList.Add("Scunthorpe");
        allowList.Add("Penistone");

        Assert.AreEqual(2, allowList.Count);

        allowList.Remove("ScUnThOrPe");

        Assert.AreEqual(1, allowList.Count);
        Assert.IsFalse(allowList.Contains("Scunthorpe"));
        Assert.IsTrue(allowList.Contains("Penistone"));
    }

    [Test]
    public void Remove_ReturnsTrue_ForExistingEntryFromTheAllowList()
    {
        var allowList = new AllowList();
        allowList.Add("Scunthorpe");
        allowList.Add("Penistone");

        Assert.AreEqual(2, allowList.Count);

        Assert.IsTrue(allowList.Remove("Scunthorpe"));
    }

    [Test]
    public void Remove_ReturnsFalse_ForNonExistingEntryFromTheAllowList()
    {
        var allowList = new AllowList();
        allowList.Add("Scunthorpe");
        allowList.Add("Penistone");

        Assert.AreEqual(2, allowList.Count);

        Assert.IsFalse(allowList.Remove("DoesNotExist"));
    }
}