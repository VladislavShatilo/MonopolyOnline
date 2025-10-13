using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class PlayerSkin : MonoBehaviourPun
{
    [SerializeField] private Image playerSkinImage;
    [SerializeField] private TextMeshProUGUI turnJailText;
    private IEventBus eventBus;
    private IPhotonViewWrapper photonViewWrapper;

    #region LIFE_CYCLE

    public void Initialize(IEventBus eventBus, IPhotonViewWrapper photonViewWrapper)
    {
        this.eventBus = eventBus;
        eventBus.Subscribe<SetTurnsJailEvent>(SetTurnJain);
        this.photonViewWrapper = photonViewWrapper;
    }

    private void Start()
    {
        turnJailText.gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        if (eventBus != null)
        {
            eventBus.Unsubscribe<SetTurnsJailEvent>(SetTurnJain);
        }
    }

    #endregion LIFE_CYCLE

    #region PUBLIC_METHODS

    public void SetColorDirect(Color c)
    {
        playerSkinImage.color = c;
    }

    #endregion PUBLIC_METHODS

    #region RPC

    [PunRPC]
    private void RPC_SetTurnJain(int turns)
    {
        if (turns == 0)
        {
            turnJailText.gameObject.SetActive(false);
        }
        else
        {
            turnJailText.gameObject.SetActive(true);
            turnJailText.text = turns.ToString();
        }
    }

    #endregion RPC

    #region CALLBACKS

    private void SetTurnJain(SetTurnsJailEvent e)
    {
        if (e.PlayerID == photonView.OwnerActorNr)
        {
            photonViewWrapper.RPC(photonView, nameof(RPC_SetTurnJain), RpcTarget.AllBuffered, e.Turns);
        }
    }

    #endregion CALLBACKS
}

