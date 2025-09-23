using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class TimerManager : ITimerManager
{
    private TurnTimer turnTimer = new TurnTimer();

    public void StartTurnTimer(int playerId, double duration) =>
        turnTimer.Start(TimerType.Turn, playerId, PhotonNetwork.Time, duration);

    public void StartAuctionTimer(int playerId, double duration) =>
        turnTimer.Start(TimerType.Auction, playerId, PhotonNetwork.Time, duration);

    public void StartTradeTimer(int playerId, double duration) =>
        turnTimer.Start(TimerType.Trade, playerId, PhotonNetwork.Time, duration);

    public (TimerType type, int playerId, float timeLeft, bool isActive, bool expired)? Tick()
    {
        return turnTimer.Tick(PhotonNetwork.Time);
    }

    public void StopTimer() => turnTimer.Stop();
}
