using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChanceService : IChanceService
{
    private readonly System.Random random;
    private List<ChanceBuff> deck; // текущая «колода»

    public ChanceService()
    {
        random = new System.Random();
        ResetDeck();
    }

    public ChanceBuff GetRandomBuff()
    {
        if (deck.Count == 0)
        {
            ResetDeck();
        }

        int index = random.Next(deck.Count);
        ChanceBuff selected = deck[index];

        // убираем карту из колоды
        deck.RemoveAt(index);

        return selected;
    }

    private void ResetDeck()
    {
        deck = new List<ChanceBuff>();

        // Здесь явно кладём все 18 баффов
        deck.AddRange(CreateBuffs());
    }

    private IEnumerable<ChanceBuff> CreateBuffs()
    {
        // MoneyGainRandom1 (2 шт)
        yield return new ChanceBuff(BuffType.MoneyGainRandom1, 250, 1000);
        yield return new ChanceBuff(BuffType.MoneyGainRandom1, 250, 1000);

        // MoneyGainRandom2 (3 шт)
        yield return new ChanceBuff(BuffType.MoneyGainRandom2, 500, 1500);
        yield return new ChanceBuff(BuffType.MoneyGainRandom2, 500, 1500);
        yield return new ChanceBuff(BuffType.MoneyGainRandom2, 500, 1500);

        // MoneyGainFixed (1 шт)
        yield return new ChanceBuff(BuffType.MoneyGainFixed, 1500, 1500);

        // MoneyLoseRandom1 (2 шт)
        yield return new ChanceBuff(BuffType.MoneyLoseRandom1, 250, 1000);
        yield return new ChanceBuff(BuffType.MoneyLoseRandom1, 250, 1000);

        // MoneyLoseRandom2 (2 шт)
        yield return new ChanceBuff(BuffType.MoneyLoseRandom2, 500, 1500);
        yield return new ChanceBuff(BuffType.MoneyLoseRandom2, 500, 1500);

        // MoneyLoseFixed (2 шт)
        yield return new ChanceBuff(BuffType.MoneyLoseFixed, 1500, 1500);
        yield return new ChanceBuff(BuffType.MoneyLoseFixed, 1500, 1500);

        // Teleport (2 шт)
        yield return new ChanceBuff(BuffType.Teleport);
        yield return new ChanceBuff(BuffType.Teleport);

        // SkipTurn (2 шт)
        yield return new ChanceBuff(BuffType.SkipTurn);
        yield return new ChanceBuff(BuffType.SkipTurn);

        // ReverseMove (1 шт)
        yield return new ChanceBuff(BuffType.ReverseMove);

        // Jail (1 шт)
        yield return new ChanceBuff(BuffType.Jail);
    }
}
