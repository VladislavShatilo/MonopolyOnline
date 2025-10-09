using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

public class AutoScrollRectTests
{
    private GameObject canvasGO;
    private GameObject scrollGO;
    private AutoScrollRect autoScroll;
    private ScrollRect scrollRect;
    private GridLayoutGroup gridLayout;
    private RectTransform content;

    [SetUp]
    public void Setup()
    {
        // Создаем Canvas
        canvasGO = new GameObject("Canvas", typeof(Canvas));
        canvasGO.GetComponent<Canvas>().renderMode = RenderMode.WorldSpace;

        // Создаем ScrollRect
        scrollGO = new GameObject("ScrollRect", typeof(ScrollRect), typeof(AutoScrollRect));
        scrollGO.transform.SetParent(canvasGO.transform);

        // Создаем Viewport
        GameObject viewportGO = new GameObject("Viewport", typeof(RectTransform), typeof(Mask), typeof(Image));
        viewportGO.transform.SetParent(scrollGO.transform);
        RectTransform viewportRT = viewportGO.GetComponent<RectTransform>();
        viewportRT.sizeDelta = new Vector2(200, 200);

        // Создаем Content
        GameObject contentGO = new GameObject("Content", typeof(RectTransform), typeof(GridLayoutGroup));
        contentGO.transform.SetParent(viewportGO.transform);
        content = contentGO.GetComponent<RectTransform>();
        content.sizeDelta = new Vector2(200, 200);

        // Настройка ScrollRect
        scrollRect = scrollGO.GetComponent<ScrollRect>();
        scrollRect.content = content;
        scrollRect.viewport = viewportRT;

        // Настройка GridLayoutGroup
        gridLayout = contentGO.GetComponent<GridLayoutGroup>();
        gridLayout.cellSize = new Vector2(50, 50);
        gridLayout.spacing = new Vector2(0, 10);
        gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        gridLayout.constraintCount = 2;

        // Получаем AutoScrollRect
        autoScroll = scrollGO.GetComponent<AutoScrollRect>();

        // Назначаем gridLayoutGroup через Reflection, т.к. поле private [SerializeField]
        typeof(AutoScrollRect)
            .GetField("gridLayoutGroup", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.SetValue(autoScroll, gridLayout);

        // Инициализация Start() вручную (т.к. MonoBehaviour.Start не вызывается автоматически в тестах)
        autoScroll.SendMessage("Start");
    }

    [TearDown]
    public void Teardown()
    {
        Object.DestroyImmediate(canvasGO);
    }

    [UnityTest]
    public IEnumerator ScrollDisabled_WhenElementsFitViewport()
    {
        for (int i = 0; i < 2; i++)
        {
            GameObject child = new GameObject("Item" + i, typeof(RectTransform));
            child.transform.SetParent(content);
        }

        yield return new WaitForEndOfFrame();

        Assert.IsFalse(scrollRect.vertical, "Scroll should be disabled when elements fit viewport");
        Assert.AreEqual(1f, scrollRect.verticalNormalizedPosition, "VerticalNormalizedPosition should reset to 1");
    }

    [UnityTest]
    public IEnumerator ScrollEnabled_WhenElementsExceedViewport()
    {
        for (int i = 0; i < 10; i++)
        {
            GameObject child = new GameObject("Item" + i, typeof(RectTransform));
            child.transform.SetParent(content);
        }

        yield return null; // ждем кадр для Update

        Assert.IsTrue(scrollRect.vertical, "Scroll should be enabled when elements exceed viewport");
    }

    [UnityTest]
    public IEnumerator VerticalNormalizedPositionResets_WhenChildCountChanges()
    {
        GameObject child = new GameObject("Item0", typeof(RectTransform));
        child.transform.SetParent(content);
        yield return new WaitForEndOfFrame();

        float firstPosition = scrollRect.verticalNormalizedPosition;

        GameObject child2 = new GameObject("Item1", typeof(RectTransform));
        child2.transform.SetParent(content);
        yield return new WaitForEndOfFrame();

        float newPosition = scrollRect.verticalNormalizedPosition;

        Assert.AreEqual(1f, newPosition, "VerticalNormalizedPosition should reset to 1 when child count changes");
        Assert.AreNotEqual(firstPosition, newPosition, "VerticalNormalizedPosition should actually update");
    }
}
