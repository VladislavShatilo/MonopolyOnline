public interface ITurnService
{
    void StartRandomTurn();
    void StartTurn(int playerId, bool isNext);
    void EndTurn();
    void RegisterDouble(int playerId);
    void SetMode(TurnMode mode);
}