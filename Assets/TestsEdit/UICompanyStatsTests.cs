using Moq;
using NUnit.Framework;
using System;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[TestFixture]
public class UICompanyStatsTests
{
    private GameObject go;
    private UICompanyStats stats;
    private TextMeshProUGUI companyName, groupName, branchPriceText;
    private TextMeshProUGUI[] rentPriceTexts;

    private Mock<IGroupColors> groupColorsMock;

    private Image topBarImage1, topBarImage2, topBarImage3;

    [SetUp]
    public void SetUp()
    {
        go = new GameObject();
        stats = go.AddComponent<UICompanyStats>();

        var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;

        // UI элементы UICompanyStats
        branchPriceText = new GameObject().AddComponent<TextMeshProUGUI>();
        typeof(UICompanyStats).GetField("branchPriceText", flags).SetValue(stats, branchPriceText);

        rentPriceTexts = new TextMeshProUGUI[5];
        for (int i = 0; i < rentPriceTexts.Length; i++)
            rentPriceTexts[i] = new GameObject().AddComponent<TextMeshProUGUI>();
        typeof(UICompanyStats).GetField("rentPriceTexts", flags).SetValue(stats, rentPriceTexts);

        // UI элементы базового класса
        topBarImage1 = new GameObject().AddComponent<Image>();
        topBarImage2 = new GameObject().AddComponent<Image>();
        companyName = new GameObject().AddComponent<TextMeshProUGUI>();
        groupName = new GameObject().AddComponent<TextMeshProUGUI>();
        var cellPriceText = new GameObject().AddComponent<TextMeshProUGUI>();
        var pledgePriceText = new GameObject().AddComponent<TextMeshProUGUI>();
        var buyoutPriceText = new GameObject().AddComponent<TextMeshProUGUI>();

        typeof(UIBaseCompanyStats).GetField("topBarImage1", flags).SetValue(stats, topBarImage1);
        typeof(UIBaseCompanyStats).GetField("topBarImage2", flags).SetValue(stats, topBarImage2);
        typeof(UIBaseCompanyStats).GetField("companyNameText", flags).SetValue(stats, companyName);
        typeof(UIBaseCompanyStats).GetField("groupNameText", flags).SetValue(stats, groupName);
        typeof(UIBaseCompanyStats).GetField("cellPriceText", flags).SetValue(stats, cellPriceText);
        typeof(UIBaseCompanyStats).GetField("pledgePriceText", flags).SetValue(stats, pledgePriceText);
        typeof(UIBaseCompanyStats).GetField("buyoutPriceText", flags).SetValue(stats, buyoutPriceText);

        // Mock для цветов
        groupColorsMock = new Mock<IGroupColors>();
        groupColorsMock.Setup(g => g.Colors).Returns(new Color[] { Color.red, Color.green, Color.blue });
    }


    [TearDown]
    public void TearDown()
    {
        GameObject.DestroyImmediate(go);
        foreach (var t in rentPriceTexts)
            GameObject.DestroyImmediate(t.gameObject);
        GameObject.DestroyImmediate(companyName.gameObject);
        GameObject.DestroyImmediate(groupName.gameObject);
        GameObject.DestroyImmediate(branchPriceText.gameObject);
    }

    [Test]
    public void SetRentPrices_ShouldUpdateAllTexts()
    {
        int[] values = { 100, 200, 300, 400, 500 };
        stats.SetRentPrices(values);

        for (int i = 0; i < rentPriceTexts.Length; i++)
            Assert.AreEqual(values[i].ToString("N0", CultureInfo.InvariantCulture), rentPriceTexts[i].text);
    }

    [Test]
    public void SetBranchPrice_ShouldUpdateText()
    {
        stats.SetBranchPrice(12345);
        Assert.AreEqual("12,345", branchPriceText.text);
    }

    [Test]
    public void SetData_ShouldUpdateAllFields()
    {
        // создаём и инициализируем все необходимые UI элементы для базового класса
        TextMeshProUGUI cellPriceText = new GameObject().AddComponent<TextMeshProUGUI>();
        TextMeshProUGUI pledgePriceText = new GameObject().AddComponent<TextMeshProUGUI>();
        TextMeshProUGUI buyoutPriceText = new GameObject().AddComponent<TextMeshProUGUI>();

        typeof(UIBaseCompanyStats).GetField("cellPriceText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(stats, cellPriceText);
        typeof(UIBaseCompanyStats).GetField("pledgePriceText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(stats, pledgePriceText);
        typeof(UIBaseCompanyStats).GetField("buyoutPriceText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(stats, buyoutPriceText);

        stats.Constuct(groupColorsMock.Object);

        // создаём данные для компании
        CompanyData data = new()
        {
            name = "MyCompany",
            group = 0, // соответствует первому цвету в мокe
            price = 1000,
            pledgePrice = 500,
            buyoutPrice = 1500,
            branchPrice = 250,
            rent = new int[] { 10, 20, 30, 40, 50 }
        };

        // вызываем метод
        stats.SetData(data);

        // проверки
        Assert.AreEqual("MyCompany", companyName.text);
        Assert.AreEqual("Perfume", groupName.text); // группа приводится к string через ToString()

        for (int i = 0; i < rentPriceTexts.Length; i++)
            Assert.AreEqual(data.rent[i].ToString("N0", CultureInfo.InvariantCulture), rentPriceTexts[i].text);

        Assert.AreEqual(data.price.ToString("N0", CultureInfo.InvariantCulture), cellPriceText.text);
        Assert.AreEqual(data.pledgePrice.ToString("N0", CultureInfo.InvariantCulture), pledgePriceText.text);
        Assert.AreEqual(data.buyoutPrice.ToString("N0", CultureInfo.InvariantCulture), buyoutPriceText.text);

        Assert.AreEqual(data.branchPrice.ToString("N0", CultureInfo.InvariantCulture), branchPriceText.text);

        // чистим созданные объекты
        GameObject.DestroyImmediate(cellPriceText.gameObject);
        GameObject.DestroyImmediate(pledgePriceText.gameObject);
        GameObject.DestroyImmediate(buyoutPriceText.gameObject);
    }
    [Test]
    public void SetRentPrices_ShouldNotThrow_WhenRentTextsNull()
    {
        typeof(UICompanyStats).GetField("rentPriceTexts", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(stats, null);

        Assert.DoesNotThrow(() => stats.SetRentPrices(new int[] { 1, 2, 3 }));
    }

    [Test]
    public void SetBranchPrice_ShouldNotThrow_WhenBranchPriceTextNull()
    {
        typeof(UICompanyStats).GetField("branchPriceText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(stats, null);

        Assert.DoesNotThrow(() => stats.SetBranchPrice(123));
    }

    [Test]
    public void Constuct_ShouldThrow_WhenGroupColorsNull()
    {
        Assert.Throws<System.ArgumentNullException>(() => stats.Constuct(null));
    }

    // ----------------------------
    // Некорректные данные
    // ----------------------------

    [Test]
    public void SetRentPrices_ShouldHandleShortArray()
    {
        int[] values = { 10, 20 }; // меньше чем 5
        stats.SetRentPrices(values);

        Assert.AreEqual("10", rentPriceTexts[0].text);
        Assert.AreEqual("20", rentPriceTexts[1].text);

    }

    [Test]
    public void SetData_ShouldHandleNullRent()
    {
        stats.Constuct(groupColorsMock.Object);

        CompanyData data = new CompanyData()
        {
            name = "NullRentCo",
            group = 0,
            rent = null,
            price = 100,
            pledgePrice = 50,
            buyoutPrice = 150,
            branchPrice = 25
        };
        Assert.Throws<ArgumentNullException>(() => stats.SetData(data));
    }

}

