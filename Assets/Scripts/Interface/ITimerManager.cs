using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ITimerManager 
{
    void  StartTurnTimer(int playerId, double duration);
    void StartAuctionTimer(int playerId, double duration);
    void StartTradeTimer(int playerId, double duration);
    (TimerType type, int playerId, float timeLeft, bool isActive, bool expired)? Tick();
}
