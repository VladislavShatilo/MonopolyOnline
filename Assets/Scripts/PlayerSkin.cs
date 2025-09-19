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
    public void Initialize(IEventBus eventBus)
    {
        this.eventBus = eventBus;
        eventBus.Subscribe<SetTurnsJailEvent>(SetTurnJain);
    }

    private void OnDestroy()
    {
        if (eventBus != null)
        {
            eventBus.Unsubscribe<SetTurnsJailEvent>(SetTurnJain);
        }
    }

    private void SetTurnJain(SetTurnsJailEvent e)
    {
        if (e.PlayerID == photonView.OwnerActorNr)
        {
            //  if (!photonView.IsMine) return;
            photonView.RPC(nameof(RPC_SetTurnJain), RpcTarget.AllBuffered, e.Turns);
        }
    }

    private void Start()
    {
        turnJailText.gameObject.SetActive(false);
    }

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

    public void SetColorDirect(Color c)
    {
        playerSkinImage.color = c;
    }
}

public class SetTurnsJailEvent
{
    public int Turns;
    public int PlayerID;

    public SetTurnsJailEvent(int playerID, int turns)
    {
        Turns = turns;
        PlayerID = playerID;
    }
}