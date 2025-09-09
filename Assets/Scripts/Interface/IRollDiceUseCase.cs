public interface IRollDiceUseCase
{
    DiceResult GetDiceResult(int playerId, bool isForJail);
    void HandleDice(int first,int second, int playerId, bool isForJail);
}
