using NUnit.Framework;
using UnityEngine;
using TMPro;
using Moq;
using System.Globalization;

[TestFixture]
public class UIFieldCompanyStatsTests
{
    private GameObject go;
    private UIFieldCompanyStats stats;

    private TextMeshProUGUI companyNameText, groupNameText;
    private TextMeshProUGUI[] fieldPriceTexts;

    private Mock<IGroupColors> groupColorsMock;

    [SetUp]
    public void SetUp()
    {
        go = new GameObject();
        stats = go.AddComponent<UIFieldCompanyStats>();

        // Создаем UI элементы
        companyNameText = new GameObject().AddComponent<TextMeshProUGUI>();
        groupNameText = new GameObject().AddComponent<TextMeshProUGUI>();

        fieldPriceTexts = new TextMeshProUGUI[3];
        for (int i = 0; i < fieldPriceTexts.Length; i++)
            fieldPriceTexts[i] = new GameObject().AddComponent<TextMeshProUGUI>();

        // Присваиваем через Reflection
        typeof(UIBaseCompanyStats).GetField("companyNameText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(stats, companyNameText);
        typeof(UIBaseCompanyStats).GetField("groupNameText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(stats, groupNameText);
        typeof(UIFieldCompanyStats).GetField("fieldPriceTexts", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(stats, fieldPriceTexts);

        // Mock для цветов
        groupColorsMock = new Mock<IGroupColors>();
        // !!! Важно: массив цветов должен быть больше, чем максимальный индекс группы
        groupColorsMock.Setup(g => g.Colors).Returns(new Color[] { Color.red, Color.green, Color.blue, Color.yellow, Color.magenta });
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

    [Test]
    public void SetFieldPrices_ShouldUpdateAllTexts()
    {
        int[] values = { 100, 200, 300 };
        stats.SetFieldPrices(values);

        for (int i = 0; i < values.Length; i++)
            Assert.AreEqual(values[i].ToString("N0", CultureInfo.InvariantCulture), fieldPriceTexts[i].text);
    }

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
}
