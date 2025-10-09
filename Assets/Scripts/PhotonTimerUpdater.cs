using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class PhotonTimerUpdater : MonoBehaviourPun
{
    private ITimerManager timerManager;
    private IEventBus eventBus;
    private IPhotonNetworkWrapper photonNetworkWrapper;
    private int lastSecond = -1;

    #region LIFE_CYCLE

    [Inject]
    public void Construct(ITimerManager timerManager, IEventBus eventBus, IPhotonNetworkWrapper photonNetworkWrapper)
    {
        this.timerManager = timerManager;
        this.eventBus = eventBus;
        this.photonNetworkWrapper = photonNetworkWrapper;
    }

    private void Update()
    {
        if (!photonNetworkWrapper.IsMasterClient) return;

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
            {
                eventBus.Publish(new TimerExpiredEvent(type, playerId));

            }
        }

    }

    #endregion LIFE_CYCLE

    #region RPC

    [PunRPC]
    private void RPC_SyncTimer(TimerType type, int playerId, float timeLeft, bool isActive)
    {
        eventBus.Publish(new TimerUpdatedEvent(type, playerId, timeLeft, isActive));
    }

    #endregion RPC


}
