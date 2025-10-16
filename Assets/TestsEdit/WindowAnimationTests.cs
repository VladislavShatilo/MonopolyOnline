using DG.Tweening;
using NUnit.Framework;
using System;
using UnityEngine;
using Object = UnityEngine.Object;

[TestFixture]
public class WindowAnimationTests
{
    private GameObject go;
    private WindowAnimation windowAnimation;
    private RectTransform rectTransform;

    [SetUp]
    public void SetUp()
    {
        go = new GameObject();
        rectTransform = go.AddComponent<RectTransform>();
        windowAnimation = go.AddComponent<WindowAnimation>();
        windowAnimation.WindowRectTransform = rectTransform;

        // Инициализация DOTween (для тестов)
        if (!DOTween.IsTweening(rectTransform))
            DOTween.Init();
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(go);
    }

    [Test]
    public void HardHideWindow_ShouldSetAnchoredPosition()
    {
        windowAnimation.HardHideWindow();
        Assert.AreEqual(new Vector2(0, 160f), rectTransform.anchoredPosition);
    }

    [Test]
    public void ShowWindow_ShouldAnimateToZero()
    {
        var tween = windowAnimation.WindowRectTransform.DOAnchorPos(Vector2.zero, 0.5f);
        tween.Complete(); // мгновенно завершить анимацию
        Assert.AreEqual(Vector2.zero, windowAnimation.WindowRectTransform.anchoredPosition);
    }

    [Test]
    public void HideWindow_ShouldAnimateToOffset()
    {
        var tween = windowAnimation.WindowRectTransform.DOAnchorPos(new Vector2(0, 160f), 0.5f);
        tween.Complete(); // мгновенно завершить анимацию
        Assert.AreEqual(new Vector2(0, 160f), windowAnimation.WindowRectTransform.anchoredPosition);
    }
   
    [Test]
    public void ShowWindow_ShouldMoveToZero()
    {
        windowAnimation.ShowWindow();
        // Завершаем все твины сразу
        DOTween.Kill(rectTransform, false);
        Assert.AreEqual(Vector2.zero, rectTransform.anchoredPosition);
    }

    [Test]
    public void HideWindow_ShouldMoveToOffset()
    {
        windowAnimation.HideWindow();

        // Завершаем все твины, связанные с rectTransform
        DOTween.Complete(rectTransform, true);

        Assert.AreEqual(new Vector2(0, 160f), rectTransform.anchoredPosition);
    }

    [Test]
    public void Methods_DoNotThrow_WhenWindowRectTransformIsNull()
    {
        windowAnimation.WindowRectTransform = null;

        Assert.DoesNotThrow(() => windowAnimation.ShowWindow());
        Assert.DoesNotThrow(() => windowAnimation.HideWindow());
        Assert.DoesNotThrow(() => windowAnimation.HardHideWindow());
    }
}
