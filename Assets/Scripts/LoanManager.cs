using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class LoanManager : MonoBehaviourPun
{
    public static LoanManager Instance;
    [Inject] private IPlayerRepository playerRepository;

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
        EventBus.Subscribe<PayLoanEvent>(PayLoan);
    }
    private void OnDisable()
    {
        EventBus.Unsubscribe<OnStartTurnLoanEvent>(OnPlayerTurnStart);
        EventBus.Unsubscribe<PayLoanEvent>(PayLoan);

    }
    [PunRPC]
    private void RPC_RequestTakeLoan(int playerId)
    {
        PlayerData player = playerRepository.GetPlayerById(playerId);
        if (player.HasLoan) return;

        Bank.Instance.AddMoney(playerId, 5000);
        player.HasLoan = true;
        player.LoanTurnsLeft = 1;
        EventBus.Publish(new OnTakeLoanEvent(player));
        EventBus.Publish(new OnUpdatePlayerCapitalEvent(player));

    }

    public void PayLoan(PayLoanEvent e)
    {
        photonView.RPC(nameof(RPC_RequestPayLoan), RpcTarget.All, e.PlayerId);

    }
    [PunRPC]
    private void RPC_RequestPayLoan(int playerId)
    {
        PlayerData player = playerRepository.GetPlayerById(playerId);

        if (!player.HasLoan) return;
        Bank.Instance.RemoveMoney(playerId, 5500);

        player.HasLoan = false;
        player.LoanTurnsLeft = 0;
        EventBus.Publish(new OnTakeLoanEvent(player));
        EventBus.Publish(new OnUpdatePlayerCapitalEvent(player));


    }
    public void OnPlayerTurnStart(OnStartTurnLoanEvent e)
    {
        PlayerData player = playerRepository.GetPlayerById(e.PlayerId);

        if (!player.HasLoan) return;

        player.LoanTurnsLeft--;

        if (player.LoanTurnsLeft <= 0)
        {
            photonView.RPC(nameof(RPC_ShowLoanWindow), PhotonNetwork.CurrentRoom.GetPlayer(player.Id), player.Id);

        }
        else
        {
            EventBus.Publish(new OnTakeLoanEvent(player));
        }
    }
    [PunRPC]
    private void RPC_ShowLoanWindow(int playerId)
    {
        PlayerData player = playerRepository.GetPlayerById(playerId);
        //UILoanPayWindow.Instance.ShowLoanWindow(player);

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