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
    private TimerData currentTimer;
    private ITimeProvider timeProvider;

    public TurnTimer(ITimeProvider timeProvider)
    {
        this.timeProvider = timeProvider;
    }

    public void Start(TimerType type, int playerId, double startTime, double duration)
    {
        currentTimer = new TimerData
        {
            type = type,
            playerId = playerId,
            startTime = startTime,
            duration = duration,
            isActive = true
        };
    }

    public (TimerType type, int playerId, float timeLeft, bool isActive, bool expired)? Tick()
    {
        if (currentTimer == null) return null;

        double elapsed = timeProvider.Now - currentTimer.startTime;
        float timeLeft = (float)(currentTimer.duration - elapsed);

        bool expired = timeLeft <= 0;
        currentTimer.isActive = !expired;

        return (currentTimer.type, currentTimer.playerId, Mathf.Max(timeLeft, 0f), currentTimer.isActive, expired);
    }

    private class TimerData
    {
        public TimerType type;
        public int playerId;
        public double startTime;
        public double duration;
        public bool isActive;
    }
}