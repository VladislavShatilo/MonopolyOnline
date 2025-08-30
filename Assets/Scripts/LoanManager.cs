using Photon.Pun;
using Photon.Pun.Demo.PunBasics;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoanManager : MonoBehaviourPun
{
    public static LoanManager Instance;
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
    public void TakeLoan(int playerId)
    {
        photonView.RPC(nameof(RPC_RequestTakeLoan), RpcTarget.All, playerId);
    }
    private void OnEnable()
    {
        EventBus.Subscribe<OnStartTurnLoanEvent>(OnPlayerTurnStart);
    }
    private void OnDisable()
    {
        EventBus.Unsubscribe<OnStartTurnLoanEvent>(OnPlayerTurnStart);
    }
    [PunRPC]
    private void RPC_RequestTakeLoan(int playerId)
    {
        PlayerData player = GameManager.Instance.GetPlayerById(playerId);
        if (player.HasLoan) return;

        Bank.Instance.AddMoney(player, 5000);
        player.HasLoan = true;
        player.LoanTurnsLeft = 1;
        EventBus.Publish(new OnTakeLoanEvent(player));
    }

    public void PayLoan(int playerId)
    {
        photonView.RPC(nameof(RPC_RequestPayLoan), RpcTarget.All, playerId);

    }
    [PunRPC]
    private void RPC_RequestPayLoan(int playerId)
    {
        PlayerData player = GameManager.Instance.GetPlayerById(playerId);

        if (!player.HasLoan) return;
        Bank.Instance.RemoveMoney(player, 5500);

        player.HasLoan = false;
        player.LoanTurnsLeft = 0;
        EventBus.Publish(new OnTakeLoanEvent(player));

    }
    public void OnPlayerTurnStart(OnStartTurnLoanEvent e)
    {
        PlayerData player = GameManager.Instance.GetPlayerById(e.PlayerId);

        if (!player.HasLoan) return;

        player.LoanTurnsLeft--;

        if (player.LoanTurnsLeft <= 0)
        {
            photonView.RPC(nameof(RPC_ShowLoanWindow), PhotonNetwork.CurrentRoom.GetPlayer(player.id), player.id);

        }
        else
        {
            EventBus.Publish(new OnTakeLoanEvent(player));
        }
    }
    [PunRPC]
    private void RPC_ShowLoanWindow(int playerId)
    {
        UILoanPayWindow.Instance.ShowLoanWindow();

    }
}
public class OnTakeLoanEvent
{
    public PlayerData PlayerData;

    public OnTakeLoanEvent(PlayerData playerData)
    {
        PlayerData = playerData;
    }
}
public class OnStartTurnLoanEvent
{
    public int PlayerId;

    public OnStartTurnLoanEvent(int playerId)
    {
        PlayerId = playerId;
    }
}