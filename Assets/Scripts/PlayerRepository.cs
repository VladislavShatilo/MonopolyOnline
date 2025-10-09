using Photon.Pun;
using System.Collections.Generic;

public class PlayerRepository : IPlayerRepository
{
    private readonly List<PlayerData> players = new();

    #region PUBLIC_METHODS

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
        if (players.Count == 0)
            throw new System.ArgumentOutOfRangeException(nameof(players), "Player list is empty.");

        int index = players.FindIndex(p => p.Id == currentId);
        if (index == -1)
            throw new System.ArgumentException($"Player with Id {currentId} not found.");

        index = (index + 1) % players.Count;
        return players[index];
    }

    #endregion PUBLIC_METHODS
}