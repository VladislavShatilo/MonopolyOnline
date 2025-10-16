using Moq;
using NUnit.Framework;
using System;
using System.Globalization;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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

        // --- Top Bar ---
        var topBarImage1 = new GameObject().AddComponent<Image>();
        var topBarImage2 = new GameObject().AddComponent<Image>();
        typeof(UIBaseCompanyStats).GetField("topBarImage1", BindingFlags.NonPublic | BindingFlags.Instance)
            .SetValue(stats, topBarImage1);
        typeof(UIBaseCompanyStats).GetField("topBarImage2", BindingFlags.NonPublic | BindingFlags.Instance)
            .SetValue(stats, topBarImage2);

        // --- Text Fields ---
        companyName = new GameObject().AddComponent<TextMeshProUGUI>();
        groupName = new GameObject().AddComponent<TextMeshProUGUI>();
        cellPriceText = new GameObject().AddComponent<TextMeshProUGUI>();
        pledgePriceText = new GameObject().AddComponent<TextMeshProUGUI>();
        buyoutPriceText = new GameObject().AddComponent<TextMeshProUGUI>();

        typeof(UIBaseCompanyStats).GetField("companyNameText", BindingFlags.NonPublic | BindingFlags.Instance)
            .SetValue(stats, companyName);
        typeof(UIBaseCompanyStats).GetField("groupNameText", BindingFlags.NonPublic | BindingFlags.Instance)
            .SetValue(stats, groupName);
        typeof(UIBaseCompanyStats).GetField("cellPriceText", BindingFlags.NonPublic | BindingFlags.Instance)
            .SetValue(stats, cellPriceText);
        typeof(UIBaseCompanyStats).GetField("pledgePriceText", BindingFlags.NonPublic | BindingFlags.Instance)
            .SetValue(stats, pledgePriceText);
        typeof(UIBaseCompanyStats).GetField("buyoutPriceText", BindingFlags.NonPublic | BindingFlags.Instance)
            .SetValue(stats, buyoutPriceText);

        // --- Dice Fields ---
        diceFieldMultiTexts = new TextMeshProUGUI[3];
        for (int i = 0; i < diceFieldMultiTexts.Length; i++)
            diceFieldMultiTexts[i] = new GameObject().AddComponent<TextMeshProUGUI>();

        typeof(UIDiceStats).GetField("diceFieldMultiTexts", BindingFlags.NonPublic | BindingFlags.Instance)
            .SetValue(stats, diceFieldMultiTexts);

        // --- Mock IGroupColors ---
        groupColorsMock = new Mock<IGroupColors>();
        groupColorsMock.Setup(g => g.Colors).Returns(new Color[] { Color.red, Color.green, Color.blue });

        stats.Constuct(groupColorsMock.Object); // теперь не упадет
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

    // ============================
    // SetDiceFieldMultiTexts tests
    // ============================

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
    public void SetDiceFieldMultiTexts_ShouldThrow_WhenArrayIsNull()
    {
        typeof(UIDiceStats).GetField("diceFieldMultiTexts", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(stats, null);

        Assert.Throws<ArgumentNullException>(() => stats.SetDiceFieldMultiTexts(new int[] { 1, 2, 3 }));
    }

    [Test]
    public void SetDiceFieldMultiTexts_ShouldHandleValuesShorterThanUIArray()
    {
        int[] values = { 5, 10 }; // меньше чем diceFieldMultiTexts.Length = 3
        stats.SetDiceFieldMultiTexts(values);

        Assert.AreEqual("5", diceFieldMultiTexts[0].text);
        Assert.AreEqual("10", diceFieldMultiTexts[1].text);
    }

    [Test]
    public void SetDiceFieldMultiTexts_ShouldHandleValuesLongerThanUIArray()
    {
        int[] values = { 1, 2, 3, 4, 5 }; // длиннее массива UI
        stats.SetDiceFieldMultiTexts(values);

        for (int i = 0; i < diceFieldMultiTexts.Length; i++)
        {
            Assert.AreEqual(values[i].ToString("N0", CultureInfo.InvariantCulture), diceFieldMultiTexts[i].text);
        }
    }

    // ============================
    // SetData tests
    // ============================

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

    [Test]
    public void SetData_ShouldThrow_WhenDataIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => stats.SetData(null));
    }
}
