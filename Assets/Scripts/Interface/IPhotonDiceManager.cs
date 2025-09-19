public interface IPhotonDiceManager
{
    void RequestDiceRoll(int playerId, bool isForJail, int cheatFirst = -1, int cheatSecond = -1);
}