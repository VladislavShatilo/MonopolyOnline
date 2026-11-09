using Photon.Pun;
using System;
using UnityEngine;
using Zenject;

public class PhotonChatService : MonoBehaviourPun, IChatService
{
    private IEventBus eventBus;
    private IPhotonNetworkWrapper photonNetworkWrapper;
    private IPhotonViewWrapper photonViewWrapper;

    #region LIFE_CYCLE

    [Inject]
    public void Construct(IEventBus eventBus, IPhotonNetworkWrapper photonNetworkWrapper, IPhotonViewWrapper photonViewWrapper)
    {
        this.eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        this.photonNetworkWrapper = photonNetworkWrapper ?? throw new ArgumentNullException(nameof(photonNetworkWrapper));
        this.photonViewWrapper = photonViewWrapper ?? throw new ArgumentNullException(nameof(photonViewWrapper));
        if (photonView == null) throw new NullReferenceException(nameof(photonView));

    }

    #endregion LIFE_CYCLE

    #region PUBLIC_METHODS

    public void SendMessage(int playerId, string text, bool isChat)
    {
        if (isChat)
        {
            photonViewWrapper.RPC(photonView, nameof(RPC_ReceiveMessage), RpcTarget.All, playerId, text);
        }
        else
        {
            if (!photonNetworkWrapper.IsMasterClient) return;
            photonViewWrapper.RPC(photonView, nameof(RPC_ReceiveMessage), RpcTarget.All, playerId, text);

        }
    }

    #endregion PUBLIC_METHODS

    #region RPC

    [PunRPC]
    private void RPC_ReceiveMessage(int playerId, string text)
    {
        eventBus.Publish(new ChatMessage(playerId, text));
    }

    #endregion RPC

}