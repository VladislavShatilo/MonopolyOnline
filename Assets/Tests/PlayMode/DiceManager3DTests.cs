using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Moq;

public class DiceManager3DTests
{
    private GameObject _gameObject;
    private DiceManager3D _diceManager;

    private Mock<DiceRoll3D> _mockDice1;
    private Mock<DiceRoll3D> _mockDice2;

    [SetUp]
    public void SetUp()
    {
        _gameObject = new GameObject();
        _diceManager = _gameObject.AddComponent<DiceManager3D>();

        // Создаем моки кубиков
        _mockDice1 = new Mock<DiceRoll3D>();
        _mockDice2 = new Mock<DiceRoll3D>();

        // Создаем реальные объекты GameObject, чтобы SetActive работал
        var diceObj1 = new GameObject();
        var diceObj2 = new GameObject();

        _mockDice1.SetupGet(d => d.gameObject).Returns(diceObj1);
        _mockDice2.SetupGet(d => d.gameObject).Returns(diceObj2);

        // Присваиваем кубики через рефлексию
        typeof(DiceManager3D)
            .GetField("dice1", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(_diceManager, _mockDice1.Object);

        typeof(DiceManager3D)
            .GetField("dice2", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(_diceManager, _mockDice2.Object);
    }

    [TearDown]
    public void TearDown()
    {
        GameObject.Destroy(_gameObject);
    }

    [UnityTest]
    public IEnumerator ShowDice_ActivatesDiceAndRollsToResult()
    {
        // Действие
        _diceManager.ShowDice(3, 5);

        yield return null; // Ждем один кадр

        // Проверяем, что кубики активны
        _mockDice1.Verify(d => d.gameObject.SetActive(true), Times.Once);
        _mockDice2.Verify(d => d.gameObject.SetActive(true), Times.Once);

        // Проверяем, что вызван RollToResult с нужными числами
        _mockDice1.Verify(d => d.RollToResult(3), Times.Once);
        _mockDice2.Verify(d => d.RollToResult(5), Times.Once);
    }
}
