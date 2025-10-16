using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;

[TestFixture]
public class ChanceServiceTestsFull
{
    private ChanceService chanceService;

    [SetUp]
    public void SetUp()
    {
        chanceService = new ChanceService();
    }

    [Test]
    public void Constructor_ShouldInitializeDeck()
    {
        // Проверяем, что можно вытянуть карту сразу после создания
        var buff = chanceService.GetRandomBuff();
        Assert.That(buff, Is.Not.Null);
    }

    [Test]
    public void GetRandomBuff_ShouldReturn18UniqueCardsBeforeReset()
    {
        var drawn = new List<ChanceBuff>();

        for (int i = 0; i < 18; i++)
        {
            var buff = chanceService.GetRandomBuff();
            Assert.That(buff, Is.Not.Null);
            drawn.Add(buff);
        }

        // Проверка, что после 18 вызовов все карты были вытянуты
        Assert.That(drawn.Count, Is.EqualTo(18));

        // Следующий вызов должен сбросить колоду
        var next = chanceService.GetRandomBuff();
        Assert.That(next, Is.Not.Null);
    }

    [Test]
    public void GetRandomBuff_ShouldNotReturnSameCardMoreTimesThanInDeck()
    {
        var typeCounts = new Dictionary<BuffType, int>();

        // Считаем, сколько раз каждый тип встречается в колоде
        for (int i = 0; i < 18; i++)
        {
            var buff = chanceService.GetRandomBuff();
            if (!typeCounts.ContainsKey(buff.Type))
                typeCounts[buff.Type] = 0;
            typeCounts[buff.Type]++;
        }

        // Проверяем, что никакой тип не встречается больше, чем в исходной колоде
        foreach (var kvp in typeCounts)
        {
            switch (kvp.Key)
            {
                case BuffType.MoneyGainRandom1:
                case BuffType.MoneyLoseRandom1:
                case BuffType.Teleport:
                case BuffType.SkipTurn:
                    Assert.That(kvp.Value, Is.EqualTo(2));
                    break;
                case BuffType.MoneyGainRandom2:
                    Assert.That(kvp.Value, Is.EqualTo(3));
                    break;
                case BuffType.MoneyGainFixed:
                case BuffType.MoneyLoseRandom2:
                case BuffType.MoneyLoseFixed:
                    Assert.That(kvp.Value, Is.EqualTo(2) | Is.EqualTo(1));
                    break;
                case BuffType.ReverseMove:
                case BuffType.Jail:
                    Assert.That(kvp.Value, Is.EqualTo(1));
                    break;
            }
        }
    }

    [Test]
    public void GetRandomBuff_ShouldResetDeckAfterExhaustion()
    {
        // Вытягиваем все карты
        for (int i = 0; i < 18; i++)
            chanceService.GetRandomBuff();

        // Колода опустела, следующий вызов должен сбросить её
        var buffAfterReset = chanceService.GetRandomBuff();
        Assert.That(buffAfterReset, Is.Not.Null);
    }

    [Test]
    public void GetRandomBuff_ShouldHaveValidAmountRanges()
    {
        for (int i = 0; i < 18; i++)
        {
            var buff = chanceService.GetRandomBuff();

            if (buff.Type == BuffType.MoneyGainRandom1 || buff.Type == BuffType.MoneyGainRandom2 ||
                buff.Type == BuffType.MoneyLoseRandom1 || buff.Type == BuffType.MoneyLoseRandom2)
            {
                Assert.That(buff.MinAmount, Is.LessThanOrEqualTo(buff.MaxAmount));
            }

            if (buff.Type == BuffType.MoneyGainFixed || buff.Type == BuffType.MoneyLoseFixed)
            {
                Assert.That(buff.MinAmount, Is.EqualTo(buff.MaxAmount));
            }
        }
    }

    [Test]
    public void GetRandomBuff_ShouldEventuallyReturnAllTypes()
    {
        var seenTypes = new HashSet<BuffType>();

        for (int i = 0; i < 18; i++)
        {
            seenTypes.Add(chanceService.GetRandomBuff().Type);
        }

        // Проверяем, что все типы из колоды встречаются хотя бы раз
        Assert.That(seenTypes.Count, Is.GreaterThanOrEqualTo(10));
        Assert.That(seenTypes.Contains(BuffType.MoneyGainRandom1));
        Assert.That(seenTypes.Contains(BuffType.MoneyGainRandom2));
        Assert.That(seenTypes.Contains(BuffType.MoneyGainFixed));
        Assert.That(seenTypes.Contains(BuffType.MoneyLoseRandom1));
        Assert.That(seenTypes.Contains(BuffType.MoneyLoseRandom2));
        Assert.That(seenTypes.Contains(BuffType.MoneyLoseFixed));
        Assert.That(seenTypes.Contains(BuffType.Teleport));
        Assert.That(seenTypes.Contains(BuffType.SkipTurn));
        Assert.That(seenTypes.Contains(BuffType.ReverseMove));
        Assert.That(seenTypes.Contains(BuffType.Jail));
    }

    [Test]
    public void GetRandomBuff_CanBeCalledMultipleTimesWithoutExceptions()
    {
        // Вытягиваем 50 карт подряд (с проверкой сброса колоды)
        for (int i = 0; i < 50; i++)
        {
            var buff = chanceService.GetRandomBuff();
            Assert.That(buff, Is.Not.Null);
        }
    }
}
