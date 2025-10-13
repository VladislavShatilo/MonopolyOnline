using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class DiceRoll3DTests
{
    private GameObject diceGO;
    private DiceRoll3D dice;

    [SetUp]
    public void SetUp()
    {
        diceGO = new GameObject();
        dice = diceGO.AddComponent<DiceRoll3D>();
    }

    [TearDown]
    public void TearDown()
    {
        GameObject.Destroy(diceGO);
    }

    [UnityTest]
    public IEnumerator RollToResult_SetsResultAndTriggersEvents()
    {
        int rolledValue = 4;
        bool startedCalled = false;
        int endedValue = -1;

        dice.OnRollStarted += () => startedCalled = true;
        dice.OnRollEnded += (value) => endedValue = value;

        dice.RollToResult(rolledValue);

        // Wait for full roll duration + align duration + small buffer
        yield return new WaitForSeconds(3f);


        // Проверяем что событие начала вызвано
        Assert.IsTrue(startedCalled, "OnRollStarted was not called");

        // Проверяем что событие окончания вызвано и Result установлен
        Assert.AreEqual(rolledValue, endedValue, "OnRollEnded value mismatch");
        Assert.AreEqual(rolledValue, dice.Result, "Dice Result mismatch");

        // Проверяем, что IsRolling сбросился
        Assert.IsFalse(dice.IsRolling, "IsRolling should be false after roll");
    }

    [UnityTest]
    public IEnumerator RollToResult_DoesNotStartIfAlreadyRolling()
    {
        dice.RollToResult(3);
        bool secondStarted = false;
        dice.OnRollStarted += () => secondStarted = true;

        // Пытаемся бросить второй раз
        dice.RollToResult(5);

        // Ждём меньше чем rollTime, чтобы убедиться, что второй вызов не сработал
        yield return new WaitForSeconds(0.1f);

        Assert.IsFalse(secondStarted, "Second roll should not start while rolling");
    }

    [UnityTest]
    public IEnumerator AlignToFace_SetsCorrectRotation()
    {
        int targetValue = 6;

        dice.RollToResult(targetValue);

        // Ждём полный roll + align
        float waitTime = 2f + 0.2f + 0.1f;
        yield return new WaitForSeconds(waitTime);

        // Проверяем, что объект повернут точно в нужную ориентацию
        Quaternion expectedRotation = Quaternion.Euler(180, 0, 0); // faceRotations[5]
        Assert.AreEqual(expectedRotation.eulerAngles, diceGO.transform.rotation.eulerAngles);
    }
}
