public interface IBankService
{
    void AddMoney(int playerId, int amount);
    bool RemoveMoney(int playerId, int amount);
    bool HasEnoughMoney(int playerId, int amount);
    bool TransferMoney(int fromPlayerId, int toPlayerId, int amount);
}
