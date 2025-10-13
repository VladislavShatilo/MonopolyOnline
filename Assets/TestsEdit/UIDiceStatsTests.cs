using NUnit.Framework;
using UnityEngine;
using TMPro;
using Moq;
using System.Globalization;

[TestFixture]
public class UIDiceStatsTests
{
    private GameObject go;
    private UIDiceStats stats;

    private TextMeshProUGUI companyName;
    private TextMeshProUGUI groupName;
    private TextMeshProUGUI cellPriceText;
    private TextMeshProUGUI pledgePriceText;
    private TextMeshProUGUI buyoutPriceText;
    private TextMeshProUGUI[] diceFieldMultiTexts;

    private Mock<IGroupColors> groupColorsMock;

    [SetUp]
    public void SetUp()
    {
        go = new GameObject();
        stats = go.AddComponent<UIDiceStats>();

        // Создаем UI поля
        companyName = new GameObject().AddComponent<TextMeshProUGUI>();
        groupName = new GameObject().AddComponent<TextMeshProUGUI>();
        cellPriceText = new GameObject().AddComponent<TextMeshProUGUI>();
        pledgePriceText = new GameObject().AddComponent<TextMeshProUGUI>();
        buyoutPriceText = new GameObject().AddComponent<TextMeshProUGUI>();

        diceFieldMultiTexts = new TextMeshProUGUI[3];
        for (int i = 0; i < diceFieldMultiTexts.Length; i++)
            diceFieldMultiTexts[i] = new GameObject().AddComponent<TextMeshProUGUI>();

        // Присваиваем через Reflection приватные поля базового класса
        typeof(UIBaseCompanyStats).GetField("companyNameText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(stats, companyName);
        typeof(UIBaseCompanyStats).GetField("groupNameText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(stats, groupName);
        typeof(UIBaseCompanyStats).GetField("cellPriceText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(stats, cellPriceText);
        typeof(UIBaseCompanyStats).GetField("pledgePriceText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(stats, pledgePriceText);
        typeof(UIBaseCompanyStats).GetField("buyoutPriceText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(stats, buyoutPriceText);

        // Присваиваем массив diceFieldMultiTexts
        typeof(UIDiceStats).GetField("diceFieldMultiTexts", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(stats, diceFieldMultiTexts);

        // Мок для цветов
        groupColorsMock = new Mock<IGroupColors>();
        groupColorsMock.Setup(g => g.Colors).Returns(new Color[] { Color.red, Color.green, Color.blue });
        stats.Constuct(groupColorsMock.Object);
    }

    [TearDown]
    public void TearDown()
    {
        GameObject.DestroyImmediate(go);
        GameObject.DestroyImmediate(companyName.gameObject);
        GameObject.DestroyImmediate(groupName.gameObject);
        GameObject.DestroyImmediate(cellPriceText.gameObject);
        GameObject.DestroyImmediate(pledgePriceText.gameObject);
        GameObject.DestroyImmediate(buyoutPriceText.gameObject);
        foreach (var t in diceFieldMultiTexts)
            GameObject.DestroyImmediate(t.gameObject);
    }

    [Test]
    public void SetDiceFieldMultiTexts_ShouldUpdateTexts()
    {
        int[] values = { 2, 4, 6 };
        stats.SetDiceFieldMultiTexts(values);

        for (int i = 0; i < values.Length; i++)
        {
            Assert.AreEqual(values[i].ToString("N0", CultureInfo.InvariantCulture), diceFieldMultiTexts[i].text);
        }
    }

    [Test]
    public void SetData_ShouldUpdateAllFields()
    {
        DiceCompanyData data = new DiceCompanyData()
        {
            name = "DiceCompany",
            group = 0,
            price = 1000,
            pledgePrice = 500,
            buyoutPrice = 1500,
            rentMultiplier = new int[] { 1, 2, 3 }
        };

        stats.SetData(data);

        Assert.AreEqual("DiceCompany", companyName.text);
        Assert.AreEqual("Perfume", groupName.text);

        Assert.AreEqual(data.price.ToString("N0", CultureInfo.InvariantCulture), cellPriceText.text);
        Assert.AreEqual(data.pledgePrice.ToString("N0", CultureInfo.InvariantCulture), pledgePriceText.text);
        Assert.AreEqual(data.buyoutPrice.ToString("N0", CultureInfo.InvariantCulture), buyoutPriceText.text);

        for (int i = 0; i < data.rentMultiplier.Length; i++)
            Assert.AreEqual(data.rentMultiplier[i].ToString("N0", CultureInfo.InvariantCulture), diceFieldMultiTexts[i].text);
    }
}
