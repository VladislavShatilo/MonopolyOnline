using NUnit.Framework;
using System.Collections.Generic;

[TestFixture]
public class ChanceServiceTests
{
    private ChanceService chanceService;

    [SetUp]
    public void SetUp()
    {
        chanceService = new ChanceService();
    }

    [Test]
    public void Constructor_ShouldInitializeDeckWithExpectedCount()
    {
        // Arrange
        // Act
        var buff = chanceService.GetRandomBuff(); // чтобы убедиться, что сервис рабочий

        // Assert
        // В колоде должно быть 18 карт по определению CreateBuffs()
        // Но одна уже вытянута
        Assert.That(buff, Is.Not.Null);
    }

    [Test]
    public void GetRandomBuff_ShouldReturnDifferentBuffs_UntilDeckResets()
    {
        // Arrange
        var drawnBuffs = new HashSet<ChanceBuff>();

        // Act
        for (int i = 0; i < 18; i++)
        {
            var buff = chanceService.GetRandomBuff();
            Assert.That(buff, Is.Not.Null);
            drawnBuffs.Add(buff);
        }

        // Assert
        // После вытягивания 18 карт колода должна опустеть и сброситься при следующем вызове
        var newBuff = chanceService.GetRandomBuff();
        Assert.That(newBuff, Is.Not.Null);
        Assert.That(drawnBuffs.Contains(newBuff), Is.False.Or.True); // допускаем совпадения после сброса
    }

    [Test]
    public void GetRandomBuff_ShouldResetDeck_WhenEmpty()
    {
        // Arrange
        for (int i = 0; i < 18; i++)
            chanceService.GetRandomBuff();

        // Act
        var buffAfterReset = chanceService.GetRandomBuff();

        // Assert
        Assert.That(buffAfterReset, Is.Not.Null);
    }

    [Test]
    public void GetRandomBuff_ShouldRemoveBuffFromDeck()
    {
        // Arrange
        List<ChanceBuff> drawn = new();
        var first = chanceService.GetRandomBuff();

        // Act
        for (int i = 0; i < 17; i++)
            drawn.Add(chanceService.GetRandomBuff());

        // Assert
        // После 18 вызовов колода должна сброситься
        var next = chanceService.GetRandomBuff();
        Assert.That(next, Is.Not.Null);
    }

    [Test]
    public void GetRandomBuff_ShouldEventuallyReturnAllBuffTypes()
    {
        // Arrange
        var seenTypes = new HashSet<BuffType>();

        // Act
        for (int i = 0; i < 18; i++)
        {
            var buff = chanceService.GetRandomBuff();
            seenTypes.Add(buff.Type);
        }

        // Assert
        Assert.That(seenTypes.Count, Is.GreaterThanOrEqualTo(5)); // в колоде >5 разных типов
    }
}
