using Photon.Pun;
using UnityEngine;
using Zenject;

public class PhotonChatService : MonoBehaviourPun, IChatService
{
    private IEventBus eventBus;

    #region LIFE_CYCLE

    [Inject]
    public void Construct(IEventBus eventBus)
    {
        this.eventBus = eventBus;
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
            if (!PhotonNetwork.IsMasterClient) return;
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