using Moq;
using NUnit.Framework;
using System;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[TestFixture]
public class UIBaseCompanyStatsTests
{
    private TestCompanyStats stats;
    private Mock<IGroupColors> groupColorsMock;
    private Image topBar1, topBar2;
    private TextMeshProUGUI companyName, groupName, cellPrice, pledgePrice, buyoutPrice;

    [SetUp]
    public void SetUp()
    {
        var go = new GameObject();
        stats = go.AddComponent<TestCompanyStats>();

        // Создаём UI элементы
        topBar1 = new GameObject().AddComponent<Image>();
        topBar2 = new GameObject().AddComponent<Image>();
        companyName = new GameObject().AddComponent<TextMeshProUGUI>();
        groupName = new GameObject().AddComponent<TextMeshProUGUI>();
        cellPrice = new GameObject().AddComponent<TextMeshProUGUI>();
        pledgePrice = new GameObject().AddComponent<TextMeshProUGUI>();
        buyoutPrice = new GameObject().AddComponent<TextMeshProUGUI>();

        // Присваиваем через Reflection, т.к. поля private
        typeof(UIBaseCompanyStats).GetField("topBarImage1", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(stats, topBar1);
        typeof(UIBaseCompanyStats).GetField("topBarImage2", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(stats, topBar2);
        typeof(UIBaseCompanyStats).GetField("companyNameText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(stats, companyName);
        typeof(UIBaseCompanyStats).GetField("groupNameText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(stats, groupName);
        typeof(UIBaseCompanyStats).GetField("cellPriceText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(stats, cellPrice);
        typeof(UIBaseCompanyStats).GetField("pledgePriceText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(stats, pledgePrice);
        typeof(UIBaseCompanyStats).GetField("buyoutPriceText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(stats, buyoutPrice);

        // Mock для цветов
        groupColorsMock = new Mock<IGroupColors>();
        groupColorsMock.Setup(g => g.Colors).Returns(new Color[] { Color.red, Color.green, Color.blue });

        stats.Constuct(groupColorsMock.Object);
    }
    [Test]
    public void SetTopBarColor_ShouldApplyCorrectColor()
    {
        stats.SetTopBarColor(1); // Зеленый
        Assert.AreEqual(Color.green, topBar1.color);
        Assert.AreEqual(Color.green, topBar2.color);
    }

    [Test]
    public void SetCompanyName_ShouldUpdateText()
    {
        stats.SetCompanyName("TestCo");
        Assert.AreEqual("TestCo", companyName.text);
    }

    [Test]
    public void SetGroupName_ShouldUpdateText()
    {
        stats.SetGroupName("GroupA");
        Assert.AreEqual("GroupA", groupName.text);
    }
    [Test]
    public void SetCellPrice_ShouldFormatText()
    {
        stats.SetCellPrice(1234567);
        Assert.AreEqual("1,234,567", cellPrice.text);
    }

    [Test]
    public void SetPledgePrice_ShouldFormatText()
    {
        stats.SetPledgePrice(8900);
        Assert.AreEqual("8,900", pledgePrice.text);
    }

    [Test]
    public void SetBuyoutPrice_ShouldFormatText()
    {
        stats.SetBuyoutPrice(45000);
        Assert.AreEqual("45,000", buyoutPrice.text);
    }
    [Test]
    public void Construct_ShouldThrow_WhenGroupColorsIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => stats.Constuct(null));
    }

    [Test]
    public void Construct_ShouldThrow_WhenTopBarImage1IsNull()
    {
        typeof(UIBaseCompanyStats)
            .GetField("topBarImage1", BindingFlags.NonPublic | BindingFlags.Instance)
            .SetValue(stats, null);
        Assert.Throws<ArgumentNullException>(() => stats.Constuct(groupColorsMock.Object));
    }
    [Test]
    public void SetTopBarColor_ShouldNotThrow_WhenImageIsNull()
    {
        typeof(UIBaseCompanyStats)
            .GetField("topBarImage1", BindingFlags.NonPublic | BindingFlags.Instance)
            .SetValue(stats, null);

        Assert.DoesNotThrow(() => stats.SetTopBarColor(0));
    }
    [Test]
    public void SetText_ShouldNotThrow_WhenTextFieldIsNull()
    {
        Assert.DoesNotThrow(() => stats.SetCompanyName(null));
    }
    [TestCase(0, "0")]
    [TestCase(-1234, "-1,234")]
    [TestCase(1000000, "1,000,000")]
    public void FormatNumber_ShouldReturnFormattedString(int value, string expected)
    {
        var method = typeof(UIBaseCompanyStats)
            .GetMethod("FormatNumber", BindingFlags.NonPublic | BindingFlags.Instance);
        string result = (string)method.Invoke(stats, new object[] { value });
        Assert.AreEqual(expected, result);
    }



}
public class TestCompanyStats : UIBaseCompanyStats { }
