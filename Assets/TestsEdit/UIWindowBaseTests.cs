using NUnit.Framework;
using UnityEngine;

public class UIWindowBaseTests
{
    private GameObject _go;
    private TestUIWindowBase _window;
    private TestWindowAnimation _anim;

    [SetUp]
    public void SetUp()
    {
        _go = new GameObject("WindowBase");
        _window = _go.AddComponent<TestUIWindowBase>();

        var animGO = new GameObject("Animation");
        _anim = animGO.AddComponent<TestWindowAnimation>();

        // ѕодставл€ем анимацию в поле windowAnimation
        _window.GetType().GetField("windowAnimation", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
               .SetValue(_window, _anim);
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(_go);
        Object.DestroyImmediate(_anim.gameObject);
    }
    [Test]
    public void ShowWindow_CallsAnimation()
    {
        _window.ShowWindow();
        Assert.IsTrue(_anim.showCalled);
    }

    [Test]
    public void HideWindow_CallsAnimation()
    {
        _window.HideWindow();
        Assert.IsTrue(_anim.hideCalled);
    }

    [Test]
    public void HardHideWindow_CallsAnimation()
    {
        _window.HardHideWindow();
        Assert.IsTrue(_anim.hardHideCalled);
    }

    [Test]
    public void ShowWindow_DoesNotThrow_WhenAnimationIsNull()
    {
        _window.GetType().GetField("windowAnimation", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
               .SetValue(_window, null);

        Assert.DoesNotThrow(() => _window.ShowWindow());
        Assert.DoesNotThrow(() => _window.HideWindow());
        Assert.DoesNotThrow(() => _window.HardHideWindow());
    }
}
public class TestUIWindowBase : UIWindowBase
{
    // ѕереопредел€ть не нужно, просто используем базовые методы
}