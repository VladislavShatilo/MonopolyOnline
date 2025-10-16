using NUnit.Framework;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;
using Object = UnityEngine.Object;

public class AutoScrollRectPlayModeTests
{
    private GameObject _go;
    private AutoScrollRect _autoScroll;
    private ScrollRect _scrollRect;
    private RectTransform _content;
    private GridLayoutGroup _grid;

    [SetUp]
    public void SetUp()
    {
        _go = new GameObject("AutoScrollTest");
        _scrollRect = _go.AddComponent<ScrollRect>();
        _autoScroll = _go.AddComponent<AutoScrollRect>();

        // Создаем viewport
        var viewportGO = new GameObject("Viewport", typeof(RectTransform));
        viewportGO.transform.SetParent(_go.transform, false);
        _scrollRect.viewport = viewportGO.GetComponent<RectTransform>();
        _scrollRect.viewport.sizeDelta = new Vector2(100, 200);

        // Создаем content
        var contentGO = new GameObject("Content", typeof(RectTransform));
        contentGO.transform.SetParent(viewportGO.transform, false); // внутри viewport
        _scrollRect.content = contentGO.GetComponent<RectTransform>();
        _content = _scrollRect.content;

        // Создаем GridLayoutGroup
        _grid = contentGO.AddComponent<GridLayoutGroup>();
        _grid.cellSize = new Vector2(50, 50);
        _grid.spacing = new Vector2(5, 5);
        _grid.constraintCount = 2;

        _autoScroll.GetType()
            .GetField("gridLayoutGroup", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(_autoScroll, _grid);
    }


    [TearDown]
    public void TearDown()
    {
        Object.Destroy(_go);
    }

    [UnityTest]
    public IEnumerator Start_ThrowsIfNoScrollRectOrContent()
    {
        var go = new GameObject("BrokenScroll");
        var autoScroll = go.AddComponent<AutoScrollRect>();

        // Ловим лог ошибки
        LogAssert.Expect(LogType.Exception, new System.Text.RegularExpressions.Regex("ArgumentNullException"));

        // Вызов Start через SendMessage
        autoScroll.SendMessage("Start");

        Object.Destroy(go);
        yield return null;
    }

    [UnityTest]
    public IEnumerator Update_DisablesVerticalIfChildCountLessThanMaxVisible()
    {
        // Arrange
        _autoScroll.SendMessage("Start");

        // Добавим меньше элементов, чем maxVisibleElements
        for (int i = 0; i < 3; i++)
        {
            var child = new GameObject($"Child{i}", typeof(RectTransform));
            child.transform.SetParent(_content, false);
        }

        // Act
        _autoScroll.SendMessage("Update");

        // Assert: вертикальная прокрутка должна быть выключена
        Assert.IsFalse(_scrollRect.vertical, "Vertical should be false when child count <= maxVisibleElements");

        // Опционально: проверяем, что количество дочерних элементов учтено
        Assert.AreEqual(3, _content.childCount);

        yield return null;
    }

    [UnityTest]
    public IEnumerator Update_EnablesVerticalIfChildCountExceedsMaxVisible()
    {
        // Arrange
        _autoScroll.SendMessage("Start");

        // Добавим больше элементов, чем maxVisibleElements
        for (int i = 0; i < 10; i++)
        {
            var child = new GameObject($"Child{i}", typeof(RectTransform));
            child.transform.SetParent(_content, false);
        }

        // Act
        _autoScroll.SendMessage("Update");

        // Assert
        Assert.IsTrue(_scrollRect.vertical, "Vertical should be true when child count > maxVisibleElements");
        yield return null;
    }

    [UnityTest]
    public IEnumerator Update_DoesNothingIfGridLayoutIsNull()
    {
        // Arrange
        _autoScroll.SendMessage("Start");

        // Убираем GridLayoutGroup
        _autoScroll.GetType()
            .GetField("gridLayoutGroup", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(_autoScroll, null);

        // Act
        _autoScroll.SendMessage("Update");

        // Assert (не должно быть ошибок и vertical остаётся по умолчанию)
        Assert.IsFalse(_scrollRect.vertical);
        yield return null;
    }
}
