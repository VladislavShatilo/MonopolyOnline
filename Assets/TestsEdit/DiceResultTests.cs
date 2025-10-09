using NUnit.Framework;
using System;

public class DiceResultTests
{
    [Test]
    public void Constructor_ShouldAssignFirstAndSecond()
    {
        var dice = new DiceResult(3, 5);

        Assert.AreEqual(3, dice.First);
        Assert.AreEqual(5, dice.Second);
    }

    [Test]
    public void IsDouble_ShouldReturnTrue_WhenNumbersAreEqual()
    {
        var dice = new DiceResult(4, 4);

        Assert.IsTrue(dice.IsDouble);
    }

    [Test]
    public void IsDouble_ShouldReturnFalse_WhenNumbersAreDifferent()
    {
        var dice = new DiceResult(2, 5);

        Assert.IsFalse(dice.IsDouble);
    }

    [Test]
    public void Sum_ShouldReturnSumOfDice()
    {
        var dice = new DiceResult(2, 6);

        Assert.AreEqual(8, dice.Sum);
    }

    // Можно раскомментировать проверки на диапазон, если вернёте проверки в конструкторе
    /*
    [Test]
    public void Constructor_ShouldThrow_WhenFirstIsInvalid()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new DiceResult(0, 3));
        Assert.Throws<ArgumentOutOfRangeException>(() => new DiceResult(7, 3));
    }

    [Test]
    public void Constructor_ShouldThrow_WhenSecondIsInvalid()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new DiceResult(3, 0));
        Assert.Throws<ArgumentOutOfRangeException>(() => new DiceResult(3, 7));
    }
    */
}
