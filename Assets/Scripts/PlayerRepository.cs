
using Photon.Pun;
using System.Collections.Generic;

public class PlayerRepository : IPlayerRepository
{
 
    private readonly List<PlayerData> players = new();

    public PlayerData GetPlayerById(int id) => players.Find(p => p.Id == id);
    public IReadOnlyList<PlayerData> GetAllPlayers() => players.AsReadOnly();

    public void AddPlayer(PlayerData player) => players.Add(player);

    public void RemovePlayer(int id)
    {
        var p = GetPlayerById(id);
        if (p != null) players.Remove(p);
    }
    public PlayerData GetNextPlayerId(int currentId)
    {
        int index = players.FindIndex(p => p.Id == currentId);
        index = (index + 1) % players.Count;
        return players[index];
    }
}