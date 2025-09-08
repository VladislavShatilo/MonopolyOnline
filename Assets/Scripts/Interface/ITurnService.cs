public interface ITurnService
{
    void StartRandomTurn();
    void StartTurn(int playerId, bool isNext);
    void EndTurn();
    void RegisterDouble(int playerId, bool isDouble);
    void SetMode(TurnMode mode);
}