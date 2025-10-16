using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;
using TMPro;
using UnityEngine.UI;
using System;

public class UIRansomJailWindowPlayModeTests
{
    private GameObject _go;
    private UIRansomJailWindow _window;

    [UnitySetUp]
    public IEnumerator SetUp()
    {
        _go = new GameObject("RansomJailWindow");
        _window = _go.AddComponent<UIRansomJailWindow>();

        // Создаем отдельные GameObject'ы для кнопок и текстов
        var ransomButtonGO = new GameObject("RansomButton");
        _window.RansomButtonPublic = ransomButtonGO.AddComponent<Button>();
        ransomButtonGO.transform.SetParent(_go.transform);

        var cantRansomButtonGO = new GameObject("CantRansomButton");
        _window.CantRansomButtonPublic = cantRansomButtonGO.AddComponent<Button>();
        cantRansomButtonGO.transform.SetParent(_go.transform);

        var ransomTextGO = new GameObject("RansomText");
        _window.RansomTextPublic = ransomTextGO.AddComponent<TextMeshProUGUI>();
        ransomTextGO.transform.SetParent(_go.transform);

        var cantRansomTextGO = new GameObject("CantRansomText");
        _window.CantRansomTextPublic = cantRansomTextGO.AddComponent<TextMeshProUGUI>();
        cantRansomTextGO.transform.SetParent(_go.transform);

        // Ждем один кадр, чтобы Start() вызвался
        yield return null;
    }


    [UnityTearDown]
    public IEnumerator TearDown()
    {
        GameObject.Destroy(_go);
        yield return null;
    }

    [UnityTest]
    public IEnumerator Show_WithZeroFine_SetsTextCorrectly()
    {
        _window.Show(1, 0, true);
        yield return null;

        Assert.AreEqual("Заплатите 0", _window.RansomTextPublic.text);
        Assert.AreEqual("Заплатите 0", _window.CantRansomTextPublic.text);
        Assert.IsTrue(_window.RansomButtonPublic.gameObject.activeSelf);
    }

    [UnityTest]
    public IEnumerator SetRansomAction_Null_DoesNotThrow()
    {
        _window.SetRansomAction(null);
        yield return null;

        // Попытка вызвать клик кнопки не должна приводить к ошибке
        Assert.DoesNotThrow(() => _window.RansomButtonPublic.onClick.Invoke());
    }
}
