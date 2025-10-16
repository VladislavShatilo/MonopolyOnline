using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class TimerManager : ITimerManager
{
    private TurnTimer turnTimer;

    public TimerManager(ITimeProvider timeProvider)
    {
        turnTimer = new TurnTimer(timeProvider);
    }

    public void StartTurnTimer(int playerId, double duration) =>
        turnTimer.Start(TimerType.Turn, playerId, 0, duration);

    public void StartAuctionTimer(int playerId, double duration) =>
        turnTimer.Start(TimerType.Auction, playerId, 0, duration);

    public void StartTradeTimer(int playerId, double duration) =>
        turnTimer.Start(TimerType.Trade, playerId, 0, duration);

    public (TimerType type, int playerId, float timeLeft, bool isActive, bool expired)? Tick()
    {
        return turnTimer.Tick();
    }

}
