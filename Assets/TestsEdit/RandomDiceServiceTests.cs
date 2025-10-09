using NUnit.Framework;

[TestFixture]
public class RandomDiceServiceTests
{
    private RandomDiceService diceService;

    [SetUp]
    public void SetUp()
    {
        diceService = new RandomDiceService();
    }

    [Test]
    public void Roll_Should_ReturnDiceResult()
    {
        // Act
        var result = diceService.Roll();

        // Assert
        Assert.IsNotNull(result);
        Assert.That(result.First, Is.InRange(1, 6));
        Assert.That(result.Second, Is.InRange(1, 6));
    }

    [Test]
    public void Roll_Should_ReturnDifferentResults_OverMultipleRolls()
    {
        // Arrange
        var results = new System.Collections.Generic.HashSet<string>();

        // Act
        for (int i = 0; i < 100; i++)
        {
            var r = diceService.Roll();
            results.Add($"{r.First}-{r.Second}");
        }

        // Assert
        // ѕровер€ем, что есть разнообразие выпадений
        Assert.Greater(results.Count, 1);
    }
}
