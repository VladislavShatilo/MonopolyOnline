using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;
using System;

public class UIPlayerStatsPlayModeTests
{
    [UnityTest]
    public IEnumerator Start_Throws_WhenAnyUIElementIsNull()
    {
        // Создаём объект без инициализации UI
        var go = new GameObject("PlayerStats");
        var stats = go.AddComponent<UIPlayerStats>();

        // Ждём один кадр, чтобы Unity вызвала Start()
        yield return null;

        // Проверяем, что будет ArgumentNullException
        LogAssert.Expect(LogType.Exception, new System.Text.RegularExpressions.Regex("ArgumentNullException"));

        // Удаляем объект
        GameObject.Destroy(go);
    }
}
