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

        // Подставляем анимацию в поле windowAnimation
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
    [Test]
    public void ShowWindowChild_OverridesAndCallsBaseAnimation()
    {
        _window.ShowWindow();
        Assert.IsTrue(_window.childShowCalled, "Переопределенный метод ShowWindow в наследнике не вызван");
        Assert.IsTrue(_anim.showCalled, "Базовая анимация ShowWindow не вызвана");
    }

    [Test]
    public void HideWindowChild_OverridesAndCallsBaseAnimation()
    {
        _window.HideWindow();
        Assert.IsTrue(_window.childHideCalled, "Переопределенный метод HideWindow в наследнике не вызван");
        Assert.IsTrue(_anim.hideCalled, "Базовая анимация HideWindow не вызвана");
    }

    [Test]
    public void HardHideWindowChild_OverridesAndCallsBaseAnimation()
    {
        _window.HardHideWindow();
        Assert.IsTrue(_window.childHardHideCalled, "Переопределенный метод HardHideWindow в наследнике не вызван");
        Assert.IsTrue(_anim.hardHideCalled, "Базовая анимация HardHideWindow не вызвана");
    }

    [Test]
    public void ShowHideHardHide_CanBeCalledMultipleTimes()
    {
        for (int i = 0; i < 3; i++)
        {
            _window.ShowWindow();
            _window.HideWindow();
            _window.HardHideWindow();
        }

        Assert.IsTrue(_window.childShowCalled);
        Assert.IsTrue(_window.childHideCalled);
        Assert.IsTrue(_window.childHardHideCalled);

        Assert.IsTrue(_anim.showCalled);
        Assert.IsTrue(_anim.hideCalled);
        Assert.IsTrue(_anim.hardHideCalled);
    }

    [Test]
    public void Methods_DoNotThrow_WhenAnimationIsNullInChild()
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
    // Переопределять не нужно, просто используем базовые методы
    public bool childShowCalled;
    public bool childHideCalled;
    public bool childHardHideCalled;

    public override void ShowWindow()
    {
        childShowCalled = true;
        base.ShowWindow();
    }

    public override void HideWindow()
    {
        childHideCalled = true;
        base.HideWindow();
    }

    public override void HardHideWindow()
    {
        childHardHideCalled = true;
        base.HardHideWindow();
    }
}