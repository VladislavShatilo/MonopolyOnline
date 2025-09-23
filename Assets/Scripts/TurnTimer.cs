using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum TimerType
{
    Turn,
    Auction,
    Trade
}
public class TurnTimer
{

    private TimerType type;
    private int playerId;
    private double startTime;
    private double duration;
    private bool isActive;

    public void Start(TimerType type, int playerId, double startTime, double duration)
    {
        this.type = type;
        this.playerId = playerId;
        this.startTime = startTime;
        this.duration = duration;
        isActive = true;
    }

    public (TimerType type, int playerId, float timeLeft, bool isActive, bool expired)? Tick(double currentTime)
    {
        if (!isActive) return null;

        double elapsed = currentTime - startTime;
        float timeLeft = Mathf.Clamp((float)(duration - elapsed), 0, (float)duration);

        if (timeLeft <= 0)
        {
            isActive = false;
            return (type, playerId, 0, false, true); // expired = true
        }

        return (type, playerId, timeLeft, true, false);
    }

    public void Stop() => isActive = false;

}
public class TimerUpdatedEvent
{
    public TimerType Type { get; }
    public int PlayerId { get; }
    public float TimeLeft { get; }
    public bool IsActive { get; }

    public TimerUpdatedEvent(TimerType type, int playerId, float timeLeft, bool isActive)
    {
        Type = type;
        PlayerId = playerId;
        TimeLeft = timeLeft;
        IsActive = isActive;
    }
}