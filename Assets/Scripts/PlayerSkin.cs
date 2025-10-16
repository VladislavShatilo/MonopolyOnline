using Photon.Pun;
using System;
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
    private ILocalPlayerService localPlayerService;

    #region LIFE_CYCLE

    public void Initialize(IEventBus eventBus, IPhotonViewWrapper photonViewWrapper, ILocalPlayerService localPlayerService)
    {
        this.eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        this.photonViewWrapper = photonViewWrapper ?? throw new ArgumentNullException(nameof(photonViewWrapper));
        this.localPlayerService = localPlayerService ?? throw new ArgumentNullException(nameof(localPlayerService));
        if (photonView == null) throw new NullReferenceException(nameof(photonView));

        eventBus.Subscribe<SetTurnsJailEvent>(SetTurnJain);

    }

    private void Start()
    {
        if (playerSkinImage == null || turnJailText == null) throw new NullReferenceException(nameof(PlayerSkin));
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
        if (playerSkinImage != null)
        {
            playerSkinImage.color = c;
        }
    }

    #endregion PUBLIC_METHODS

    #region RPC

    [PunRPC]
    private void RPC_SetTurnJain(int turns)
    {
        if (turnJailText == null) return;

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
        if (e.PlayerID == localPlayerService.GetLocalPlayerId())
        {
            photonViewWrapper.RPC(photonView, nameof(RPC_SetTurnJain), RpcTarget.AllBuffered, e.Turns);
        }
    }

    #endregion CALLBACKS
}

