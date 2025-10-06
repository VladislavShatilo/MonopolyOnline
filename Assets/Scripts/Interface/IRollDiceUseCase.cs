using System.Collections;

public interface IRollDiceUseCase
{
    DiceResult GetDiceResult(int playerId, bool isForJail);
    IEnumerator HandleDice(int first, int second, int playerId, bool isForJail);
}
