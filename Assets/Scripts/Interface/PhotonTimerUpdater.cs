using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class PhotonTimerUpdater : MonoBehaviourPun
{
    private ITimerManager timerManager;
    private IEventBus eventBus;
    private int lastSecond = -1;

    [Inject]
    public void Construct(ITimerManager timerManager, IEventBus eventBus)
    {
        this.timerManager = timerManager;
        this.eventBus = eventBus;
    }

    private void Update()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        var result = timerManager.Tick();
        if (result.HasValue)
        {
            var (type, playerId, timeLeft, isActive, expired) = result.Value;

            int currentSecond = Mathf.CeilToInt(timeLeft);
            if (currentSecond != lastSecond || expired)
            {
                lastSecond = currentSecond;

                eventBus.Publish(new TimerUpdatedEvent(type, playerId, timeLeft, isActive));
                photonView.RPC(nameof(RPC_SyncTimer), RpcTarget.Others, type, playerId, timeLeft, isActive);
            }

            if (expired)
                eventBus.Publish(new TimerExpiredEvent(type, playerId));
        }

    }

    [PunRPC]
    private void RPC_SyncTimer(TimerType type, int playerId, float timeLeft, bool isActive)
    {
        eventBus.Publish(new TimerUpdatedEvent(type, playerId, timeLeft, isActive));
    }
}
public class TimerExpiredEvent
{
    public TimerType Type { get; }
    public int PlayerId { get; }

    public TimerExpiredEvent(TimerType type, int playerId)
    {
        Type = type;
        PlayerId = playerId;
    }
}