using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Moq;

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
}
public class TestCompanyStats : UIBaseCompanyStats { }
