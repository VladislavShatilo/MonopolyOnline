public interface IJailService
{
    void SendPlayerToJail(int playerId);
    void ReleasePlayer(int playerId, bool payFine);
    void TryReleaseByDice(int playerId, int firstDice, int secondDice);
    int GetTurnsLeft(int playerId);
}
