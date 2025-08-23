using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JailManager : MonoBehaviourPun
{
    public static JailManager Instance { get; private set; }
    private const int JAIL_TURNS = 3;
    private int playerID;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
    private void Start()
    {
        EventBus.Publish(new SetTurnsJailEvent(PhotonNetwork.LocalPlayer.ActorNumber, 0));

    }
    private void OnEnable()
    {
        EventBus.Subscribe<StartTurnJailEvent>(StartTurnJail);
    }
    private void OnDisable()
    {
        EventBus.Unsubscribe<StartTurnJailEvent>(StartTurnJail);

    }
    public void GoToJail(int playerID)
    {
        this.playerID = playerID;
        PlayerData player = GameManager.Instance.GetPlayerById(playerID);
        player.IsInJail = true;
        player.JailTurnsLeft = JAIL_TURNS;
        //if (!PhotonNetwork.IsMasterClient) return;
        if (PhotonNetwork.LocalPlayer.ActorNumber == playerID)
        {
            EventBus.Publish(new MoveToJailEvent(playerID));
            EventBus.Publish(new SetTurnsJailEvent(playerID, player.JailTurnsLeft));
        }
        TurnManager.Instance.RequestEndTurn();


    }
    public void JailOffer(int playerId)
    {
        this.playerID = playerId;
        if (!PhotonNetwork.IsMasterClient) return;
        photonView.RPC(nameof(RPC_ShowJailOffer), PhotonNetwork.CurrentRoom.GetPlayer(playerId));
    }

    [PunRPC]
    private void RPC_ShowJailOffer()
    {
        UIJailWindow.Instance.ShowWindow();
    }
    private void StartTurnJail(StartTurnJailEvent e)
    {
        PlayerData player = GameManager.Instance.GetPlayerById(e.PlayerId);
        if (e.PlayerId != playerID && !player.IsInJail && player.JailTurnsLeft == JAIL_TURNS) return;
       
      
           // ProcessTurn(player);

            if (player.IsInJail)
            {
                Debug.Log(e.PlayerId + " в тюрьме");
                JailOffer(e.PlayerId);
                EventBus.Publish(new SetTurnsJailEvent(e.PlayerId, player.JailTurnsLeft));


                // игрок пропускает ход
                // EventBus.Publish(new PlayerSkippedTurnEvent(player.id, "In Jail"));
                //EndTurn(player.Id);
                return;
            }
        
    }
    public void ReleaseFromJail(PlayerData player)
    {
        player.IsInJail = false;
        player.JailTurnsLeft = 0;
        EventBus.Publish(new PlayerReleasedFromJailEvent(player.id));
    }

    public void ProcessTurn(PlayerData player)
    {
        if (!player.IsInJail) return;

        if (player.JailTurnsLeft > 0)
        {
            player.JailTurnsLeft--;

            if (player.JailTurnsLeft == 0)
            {
                ReleaseFromJail(player);
            }
        }
    }
}

public class PlayerMovedToJailEvent
{
    public int PlayerID;
    public int JailCellIndex;

    public PlayerMovedToJailEvent(int playerId, int jailCellIndex)
    {
        PlayerID = playerId;
        JailCellIndex = jailCellIndex;
    }
}

public class PlayerReleasedFromJailEvent
{
    public int PlayerID;

    public PlayerReleasedFromJailEvent(int playerId)
    {
        PlayerID = playerId;
    }
}

public class PlayerSkippedTurnEvent
{
    public int PlayerID;
    public string Reason;
    public PlayerSkippedTurnEvent(int playerId, string reason)
    {
        PlayerID = playerId;
        Reason = reason;
    }
}