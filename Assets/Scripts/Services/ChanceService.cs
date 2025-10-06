using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChanceService : IChanceService
{
    private readonly System.Random random;
    private List<ChanceBuff> deck;

    #region LIFE_CYCLE

    public ChanceService()
    {
        random = new System.Random();
        ResetDeck();
    }

    #endregion LIFE_CYCLE

    #region PUBLIC_METHODS

    public ChanceBuff GetRandomBuff()
    {
        if (deck.Count == 0)
        {
            ResetDeck();
        }

        int index = random.Next(deck.Count);
        ChanceBuff selected = deck[index];

        deck.RemoveAt(index);

        return selected;
    }

    #endregion PUBLIC_METHODS

    #region PRIVATE_METHODS

    private void ResetDeck()
    {
        deck = new List<ChanceBuff>();

        deck.AddRange(CreateBuffs());
    }

    private IEnumerable<ChanceBuff> CreateBuffs()
    {
        yield return new ChanceBuff(BuffType.MoneyGainRandom1, 250, 1000);
        yield return new ChanceBuff(BuffType.MoneyGainRandom1, 250, 1000);

        yield return new ChanceBuff(BuffType.MoneyGainRandom2, 500, 1500);
        yield return new ChanceBuff(BuffType.MoneyGainRandom2, 500, 1500);
        yield return new ChanceBuff(BuffType.MoneyGainRandom2, 500, 1500);

        yield return new ChanceBuff(BuffType.MoneyGainFixed, 1500, 1500);

        yield return new ChanceBuff(BuffType.MoneyLoseRandom1, 250, 1000);
        yield return new ChanceBuff(BuffType.MoneyLoseRandom1, 250, 1000);

        yield return new ChanceBuff(BuffType.MoneyLoseRandom2, 500, 1500);
        yield return new ChanceBuff(BuffType.MoneyLoseRandom2, 500, 1500);

        yield return new ChanceBuff(BuffType.MoneyLoseFixed, 1500, 1500);
        yield return new ChanceBuff(BuffType.MoneyLoseFixed, 1500, 1500);

        yield return new ChanceBuff(BuffType.Teleport);
        yield return new ChanceBuff(BuffType.Teleport);

        yield return new ChanceBuff(BuffType.SkipTurn);
        yield return new ChanceBuff(BuffType.SkipTurn);

        yield return new ChanceBuff(BuffType.ReverseMove);

        yield return new ChanceBuff(BuffType.Jail);
    }

    #endregion PRIVATE_METHODS

}
