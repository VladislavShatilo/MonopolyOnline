
using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class PlayerSkin : MonoBehaviourPun
{
    [SerializeField] private Image playerSkinImage;
    [SerializeField] private TextMeshProUGUI turnJailText;

    private void OnEnable()
    {
        EventBus.Subscribe<SetTurnsJailEvent>(SetTurnJain);
    }
    private void OnDisable()
    {
        EventBus.Unsubscribe<SetTurnsJailEvent>(SetTurnJain);

    }
    private void SetTurnJain(SetTurnsJailEvent e)
    {
        if (!photonView.IsMine) return;
        photonView.RPC(nameof(RPC_SetTurnJain), RpcTarget.AllBuffered, e.Turns);

    }
    private void Start()
    {
        turnJailText.gameObject.SetActive(false);
    }

    [PunRPC]
    private void RPC_SetTurnJain(int turns)
    {
        if(turns == 0)
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

    public SetTurnsJailEvent( int playerID, int turns)
    {
        Turns = turns;
        PlayerID = playerID;
    }
}