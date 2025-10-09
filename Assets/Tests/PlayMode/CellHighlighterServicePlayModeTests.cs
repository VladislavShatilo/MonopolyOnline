using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;
using Moq;

public class CellHighlighterServicePlayModeTests
{
    private CellHighlighterService highlighterService;
    private Mock<IBoardService> boardServiceMock;
    private GameObject fadeImage;
    private GameObject dice1GO;
    private GameObject dice2GO;

    [SetUp]
    public void Setup()
    {
        fadeImage = new GameObject("FadeImage");
        dice1GO = new GameObject("Dice1");
        dice2GO = new GameObject("Dice2");

        var go = new GameObject("HighlighterService");
        highlighterService = go.AddComponent<CellHighlighterService>();

        typeof(CellHighlighterService)
            .GetField("fadeImage", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(highlighterService, fadeImage);
        typeof(CellHighlighterService)
            .GetField("dice1GO", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(highlighterService, dice1GO);
        typeof(CellHighlighterService)
            .GetField("dice2GO", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(highlighterService, dice2GO);

        boardServiceMock = new Mock<IBoardService>();
        var cellPrefab = new GameObject("Cell");
        cellPrefab.AddComponent<RectTransform>();
        boardServiceMock.Setup(b => b.GetCellGameObject(It.IsAny<int>())).Returns(cellPrefab);

        highlighterService.Construct(boardServiceMock.Object);
    }

    [UnityTest]
    public IEnumerator HideHighlight_DisablesFadeImage_AndDice_DestroyTempCell()
    {
        // Показываем подсветку, создается tempCell
        highlighterService.ShowHighlight(1);
        var tempCell = fadeImage.transform.GetChild(0).gameObject;

        // Ждём один кадр, чтобы Unity успела создать объект
        yield return null;

        // Скрываем
        highlighterService.HideHighlight();
        yield return null;

        Assert.IsFalse(fadeImage.activeSelf);
        Assert.IsFalse(dice1GO.activeSelf);
        Assert.IsFalse(dice2GO.activeSelf);

        // Проверяем уничтожение tempCell
        Assert.IsTrue(tempCell == null || tempCell.Equals(null));
    }
}
