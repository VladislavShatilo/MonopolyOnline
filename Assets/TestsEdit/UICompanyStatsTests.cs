using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Moq;
using System.Globalization;

[TestFixture]
public class UICompanyStatsTests
{
    private GameObject go;
    private UICompanyStats stats;
    private TextMeshProUGUI companyName, groupName, branchPriceText;
    private TextMeshProUGUI[] rentPriceTexts;

    private Mock<IGroupColors> groupColorsMock;

    [SetUp]
    public void SetUp()
    {
        go = new GameObject();
        stats = go.AddComponent<UICompanyStats>();

        // Создаём UI элементы
        companyName = new GameObject().AddComponent<TextMeshProUGUI>();
        groupName = new GameObject().AddComponent<TextMeshProUGUI>();
        branchPriceText = new GameObject().AddComponent<TextMeshProUGUI>();

        rentPriceTexts = new TextMeshProUGUI[5];
        for (int i = 0; i < rentPriceTexts.Length; i++)
            rentPriceTexts[i] = new GameObject().AddComponent<TextMeshProUGUI>();

        // Присваиваем через Reflection
        typeof(UIBaseCompanyStats).GetField("companyNameText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(stats, companyName);
        typeof(UIBaseCompanyStats).GetField("groupNameText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(stats, groupName);
        typeof(UICompanyStats).GetField("branchPriceText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(stats, branchPriceText);
        typeof(UICompanyStats).GetField("rentPriceTexts", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(stats, rentPriceTexts);

        // Mock для цветов
        groupColorsMock = new Mock<IGroupColors>();
        groupColorsMock.Setup(g => g.Colors).Returns(new Color[] { Color.red, Color.green, Color.blue });
        stats.Constuct(groupColorsMock.Object);
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

        // создаём данные для компании
        CompanyData data = new CompanyData()
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

}

