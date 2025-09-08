
using System.Collections.Generic;

public interface IPlayerRepository
{
    PlayerData GetPlayerById(int id);
    IReadOnlyList<PlayerData> GetAllPlayers();
    void AddPlayer(PlayerData player);
    void RemovePlayer(int id);
    PlayerData GetNextPlayerId(int currentId);
}
