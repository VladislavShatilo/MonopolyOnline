using Photon.Pun;
using UnityEngine;
using Zenject;

public class PhotonChatService : MonoBehaviourPun, IChatService
{
    private IEventBus eventBus;
    private IPhotonNetworkWrapper photonNetworkWrapper;
    #region LIFE_CYCLE

    [Inject]
    public void Construct(IEventBus eventBus, IPhotonNetworkWrapper photonNetworkWrapper)
    {
        this.eventBus = eventBus;
        this.photonNetworkWrapper = photonNetworkWrapper;
    }

    #endregion LIFE_CYCLE

    #region PUBLIC_METHODS

    public void SendMessage(int playerId, string text, bool isChat)
    {
        if (isChat)
        {
            photonView.RPC(nameof(RPC_ReceiveMessage), RpcTarget.All, playerId, text);
        }
        else
        {
            if (!photonNetworkWrapper.IsMasterClient) return;
            photonView.RPC(nameof(RPC_ReceiveMessage), RpcTarget.All, playerId, text);
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