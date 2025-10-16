using System.Collections;

public interface IRollDiceUseCase
{
    DiceResult GetDiceResult();
    IEnumerator HandleDice(int first, int second, int playerId, bool isForJail);
}
