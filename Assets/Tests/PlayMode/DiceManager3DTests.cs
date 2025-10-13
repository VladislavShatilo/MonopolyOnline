using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;
using Moq;

public class DiceManager3DTests
{
    private GameObject gameObject;
    private DiceManager3D diceManager;


    [SetUp]
    public void SetUp()
    {
        gameObject = new GameObject();
        diceManager = gameObject.AddComponent<DiceManager3D>();
   
        // Создаём заглушки для кубиков
        var dice1Obj = new GameObject("Dice1");
        dice1Obj.AddComponent<DiceRoll3D>();
        var dice2Obj = new GameObject("Dice2");
        dice2Obj.AddComponent<DiceRoll3D>();

        // Присваиваем кубики через Reflection
        SetPrivateField("dice1", dice1Obj.GetComponent<DiceRoll3D>());
        SetPrivateField("dice2", dice2Obj.GetComponent<DiceRoll3D>());
    }

    private void SetPrivateField(string name, object value)
    {
        typeof(DiceManager3D).GetField(name, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.SetValue(diceManager, value);
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(gameObject);
    }

    [UnityTest]
    public IEnumerator ShowDice_ActivatesDice_AndRollsToResult()
    {
        diceManager.ShowDice(3, 5);
        yield return new WaitForSeconds(2f);

        var dice1Field = typeof(DiceManager3D).GetField("dice1", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var dice2Field = typeof(DiceManager3D).GetField("dice2", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        var dice1 = dice1Field.GetValue(diceManager) as DiceRoll3D;
        var dice2 = dice2Field.GetValue(diceManager) as DiceRoll3D;

        Assert.IsTrue(dice1.gameObject.activeSelf);
        Assert.IsTrue(dice2.gameObject.activeSelf);

        Assert.AreEqual(3, dice1.Result);
        Assert.AreEqual(5, dice2.Result);
    }

   
}
