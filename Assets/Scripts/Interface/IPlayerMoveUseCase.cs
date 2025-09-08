public interface IPlayerMoveUseCase
{
    void MovePlayer(int playerId, int steps, bool forward);
    void TeleportPlayer(int playerId, int targetCellIndex);
    void MovePlayerToJail(int playerId);
}