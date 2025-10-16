using NUnit.Framework;
using UnityEngine;
using TMPro;
using Moq;
using System;
using System.Globalization;
using UnityEngine.UI;

[TestFixture]
public class UIFieldCompanyStatsTests
{
    private GameObject go;
    private UIFieldCompanyStats stats;

    private TextMeshProUGUI companyNameText, groupNameText;
    private TextMeshProUGUI[] fieldPriceTexts;
    private Image topBarImage1, topBarImage2;
    private TextMeshProUGUI cellPriceText, pledgePriceText, buyoutPriceText;
    private Mock<IGroupColors> groupColorsMock;

    [SetUp]
    public void SetUp()
    {
        go = new GameObject();
        stats = go.AddComponent<UIFieldCompanyStats>();

        // Создаем UI элементы
        companyNameText = new GameObject().AddComponent<TextMeshProUGUI>();
        groupNameText = new GameObject().AddComponent<TextMeshProUGUI>();

        // Новые поля
        topBarImage1 = new GameObject().AddComponent<Image>();
        topBarImage2 = new GameObject().AddComponent<Image>();
        cellPriceText = new GameObject().AddComponent<TextMeshProUGUI>();
        pledgePriceText = new GameObject().AddComponent<TextMeshProUGUI>();
        buyoutPriceText = new GameObject().AddComponent<TextMeshProUGUI>();

        fieldPriceTexts = new TextMeshProUGUI[3];
        for (int i = 0; i < fieldPriceTexts.Length; i++)
            fieldPriceTexts[i] = new GameObject().AddComponent<TextMeshProUGUI>();

        // Присваиваем через Reflection все необходимые поля
        typeof(UIBaseCompanyStats).GetField("companyNameText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(stats, companyNameText);
        typeof(UIBaseCompanyStats).GetField("groupNameText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(stats, groupNameText);
        typeof(UIBaseCompanyStats).GetField("cellPriceText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(stats, cellPriceText);
        typeof(UIBaseCompanyStats).GetField("pledgePriceText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(stats, pledgePriceText);
        typeof(UIBaseCompanyStats).GetField("buyoutPriceText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(stats, buyoutPriceText);
        typeof(UIBaseCompanyStats).GetField("topBarImage1", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(stats, topBarImage1);
        typeof(UIBaseCompanyStats).GetField("topBarImage2", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(stats, topBarImage2);

        typeof(UIFieldCompanyStats).GetField("fieldPriceTexts", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(stats, fieldPriceTexts);

        // Mock для цветов
        groupColorsMock = new Mock<IGroupColors>();
        groupColorsMock.Setup(g => g.Colors).Returns(new Color[] { Color.red, Color.green, Color.blue, Color.yellow });
        stats.Constuct(groupColorsMock.Object);
    }
    [TearDown]
    public void TearDown()
    {
        GameObject.DestroyImmediate(go);
        GameObject.DestroyImmediate(companyNameText.gameObject);
        GameObject.DestroyImmediate(groupNameText.gameObject);
        foreach (var t in fieldPriceTexts)
            GameObject.DestroyImmediate(t.gameObject);
    }

    #region SetFieldPrices Tests

    [Test]
    public void SetFieldPrices_ShouldUpdateAllTexts()
    {
        int[] values = { 100, 200, 300 };
        stats.SetFieldPrices(values);

        for (int i = 0; i < values.Length; i++)
            Assert.AreEqual(values[i].ToString("N0", CultureInfo.InvariantCulture), fieldPriceTexts[i].text);
    }

    [Test]
    public void SetFieldPrices_ShouldNotThrow_WhenValuesShorterThanFieldPriceTexts()
    {
        int[] values = { 100 }; // меньше длины массива
        stats.SetFieldPrices(values);

        Assert.AreEqual("100", fieldPriceTexts[0].text);
       
    }

    [Test]
    public void SetFieldPrices_ShouldUpdateOnlyAvailableFields_WhenValuesLongerThanFieldPriceTexts()
    {
        int[] values = { 100, 200, 300, 400, 500 }; // длиннее массива
        stats.SetFieldPrices(values);

        for (int i = 0; i < fieldPriceTexts.Length; i++)
            Assert.AreEqual(values[i].ToString("N0", CultureInfo.InvariantCulture), fieldPriceTexts[i].text);
    }

    [Test]
    public void SetFieldPrices_ShouldThrow_WhenFieldPriceTextsIsNull()
    {
        typeof(UIFieldCompanyStats).GetField("fieldPriceTexts", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(stats, null);

        Assert.Throws<ArgumentNullException>(() => stats.SetFieldPrices(new int[] { 1, 2, 3 }));
    }

    #endregion

    #region SetData Tests

    [Test]
    public void SetData_ShouldUpdateAllFields()
    {
        FieldCompanyData data = new FieldCompanyData()
        {
            name = "FieldCompany",
            group = 0, // индекс в массиве цветов
            price = 1000,
            pledgePrice = 500,
            buyoutPrice = 1500,
            rentField = new int[] { 10, 20, 30 }
        };

        stats.SetData(data);

        Assert.AreEqual("FieldCompany", companyNameText.text);
        Assert.AreEqual("Perfume", groupNameText.text);

        for (int i = 0; i < data.rentField.Length; i++)
            Assert.AreEqual(data.rentField[i].ToString("N0", CultureInfo.InvariantCulture), fieldPriceTexts[i].text);

        var cellPriceText = (TextMeshProUGUI)typeof(UIBaseCompanyStats)
            .GetField("cellPriceText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .GetValue(stats);
        Assert.AreEqual(data.price.ToString("N0", CultureInfo.InvariantCulture), cellPriceText.text);

        var pledgePriceText = (TextMeshProUGUI)typeof(UIBaseCompanyStats)
            .GetField("pledgePriceText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .GetValue(stats);
        Assert.AreEqual(data.pledgePrice.ToString("N0", CultureInfo.InvariantCulture), pledgePriceText.text);

        var buyoutPriceText = (TextMeshProUGUI)typeof(UIBaseCompanyStats)
            .GetField("buyoutPriceText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .GetValue(stats);
        Assert.AreEqual(data.buyoutPrice.ToString("N0", CultureInfo.InvariantCulture), buyoutPriceText.text);
    }

    [Test]
    public void SetData_ShouldThrow_WhenDataIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => stats.SetData(null));
    }

    [Test]
    public void SetData_ShouldThrow_WhenGroupIndexOutOfRange()
    {
        FieldCompanyData data = new FieldCompanyData()
        {
            name = "FieldCompany",
            group = (CompanyGroup)10, // больше, чем массив цветов
            price = 100,
            pledgePrice = 50,
            buyoutPrice = 150,
            rentField = new int[] { 1, 2, 3 }
        };

        Assert.Throws<ArgumentOutOfRangeException>(() => stats.SetData(data));
    }

    #endregion
}
