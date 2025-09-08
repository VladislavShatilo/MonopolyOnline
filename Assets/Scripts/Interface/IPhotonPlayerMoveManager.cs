public interface IPhotonPlayerMoveManager
{
    void RequestMove(OnPlayerMoveEvent e);
    void RequestTeleport(int playerId, int cellIndex);
}