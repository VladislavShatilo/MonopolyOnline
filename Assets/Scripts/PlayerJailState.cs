public class PlayerJailState
{
    public int PlayerId { get; }
    public bool IsInJail { get; private set; }
    public int TurnsLeft { get; private set; }

    public PlayerJailState(int playerId)
    {
        PlayerId = playerId;
        IsInJail = false;
        TurnsLeft = 0;
    }

    public void SendToJail(JailRules rules)
    {
        IsInJail = true;
        TurnsLeft = rules.MaxTurns;
    }

    public void Release()
    {
        IsInJail = false;
        TurnsLeft = 0;
    }

    public void DecreaseTurn()
    {
        if (TurnsLeft > 0)
            TurnsLeft--;
    }
}