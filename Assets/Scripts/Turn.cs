using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Turn
{
    public int CurrentPlayerId { get; private set; }
    public TurnMode Mode { get; private set; } = TurnMode.Normal;
    public bool IsActive { get; private set; }

    private HashSet<int> extraTurnPlayers = new HashSet<int>();

    public void StartTurn(int playerId)
    {
        CurrentPlayerId = playerId;
        IsActive = true;
        Mode = TurnMode.Normal;
    }

    public void EndTurn()
    {
        IsActive = false;
    }

    public void SetMode(TurnMode mode)
    {
        Mode = mode;
    }

    public void AddExtraTurn(int playerId)
    {
        extraTurnPlayers.Add(playerId);
    }

    public bool HasExtraTurn(int playerId) => extraTurnPlayers.Contains(playerId);
    public void RemoveExtraTurn(int playerId) => extraTurnPlayers.Remove(playerId);
}

