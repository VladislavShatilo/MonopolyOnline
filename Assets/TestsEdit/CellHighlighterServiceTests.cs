using NUnit.Framework;
using UnityEngine;
using Moq;

public class CellHighlighterServiceTests 
{
    private CellHighlighterService highlighterService;
    private Mock<IBoardService> boardServiceMock;

    private GameObject fadeImage;
    private GameObject dice1GO;
    private GameObject dice2GO;

    [SetUp]
    public void Setup()
    {
        // Создаём фейковые объекты
        fadeImage = new GameObject("FadeImage");
        dice1GO = new GameObject("Dice1");
        dice2GO = new GameObject("Dice2");

        // Изначально выключены
        fadeImage.SetActive(false);
        dice1GO.SetActive(false);
        dice2GO.SetActive(false);

        // Создаём сервис
        var go = new GameObject("HighlighterService");
        highlighterService = go.AddComponent<CellHighlighterService>();

        // Присваиваем поля
        typeof(CellHighlighterService)
            .GetField("fadeImage", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(highlighterService, fadeImage);
        typeof(CellHighlighterService)
            .GetField("dice1GO", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(highlighterService, dice1GO);
        typeof(CellHighlighterService)
            .GetField("dice2GO", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(highlighterService, dice2GO);

        // Мокаем IBoardService
        boardServiceMock = new Mock<IBoardService>();
        var cellPrefab = new GameObject("Cell");
        cellPrefab.AddComponent<RectTransform>();
        boardServiceMock.Setup(b => b.GetCellGameObject(It.IsAny<int>())).Returns(cellPrefab);

        // Инжектим зависимость
        highlighterService.Construct(boardServiceMock.Object);
    }

    [Test]
    public void ShowHighlight_EnablesFadeImage_AndInstantiatesCell()
    {
        // Act
        highlighterService.ShowHighlight(1);

        // Assert
        Assert.IsTrue(fadeImage.activeSelf);

        // Проверим, что внутри fadeImage есть дочерний объект
        Assert.AreEqual(1, fadeImage.transform.childCount);
        Assert.AreEqual("Cell(Clone)", fadeImage.transform.GetChild(0).name);

        // Проверим смещение
        var rect = fadeImage.transform.GetChild(0).GetComponent<RectTransform>();
        Assert.AreEqual(45f, rect.position.x - 0f); // исходная позиция 0,0 + distanceCorrection
        Assert.AreEqual(-45f, rect.position.y - 0f);
    }

}
