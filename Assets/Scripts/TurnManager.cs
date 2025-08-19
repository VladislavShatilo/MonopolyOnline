using Photon.Pun;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TurnManager : MonoBehaviourPunCallbacks
{
    public static TurnManager Instance { get; private set; }

    [Header("Settings")]
    [SerializeField] private float turnDuration = 90f;
    [SerializeField] private DiceManagerPhoton diceManager;

    private double turnStartTime;
    private int currentTurnPlayerId;
    private bool isTurnActive = false;

    private Dictionary<int, UIPlayerStats> playerStatsDict = new Dictionary<int, UIPlayerStats>();
    public int CurrentTurnPlayerId => currentTurnPlayerId;

    public event Action<int> OnTurnStarted;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Update()
    {
        if (!isTurnActive) return;

        double elapsed = PhotonNetwork.Time - turnStartTime;
        float timeLeft = Mathf.Clamp((float)(turnDuration - elapsed), 0, turnDuration);

        UpdateTurnTimer(timeLeft);

        if (timeLeft <= 0f)
            EndTurnInternal();
    }

    #region Player UI

    public void RegisterPlayerUI(int playerId, UIPlayerStats uiStats)
    {
        if (!playerStatsDict.ContainsKey(playerId))
            playerStatsDict.Add(playerId, uiStats);
    }

    private void UpdateTurnTimer(float timeLeft)
    {
        foreach (var kvp in playerStatsDict)
        {
            kvp.Value.SetTurnActive(kvp.Key == currentTurnPlayerId);
            if (kvp.Key == currentTurnPlayerId)
                kvp.Value.UpdateTurnTimer(timeLeft);
        }
    }

    #endregion

    #region Turn Management

    public void StartRandomTurn()
    {
        if (!PhotonNetwork.IsMasterClient || playerStatsDict.Count == 0) return;
        int randomPlayerId = playerStatsDict.Keys.ElementAt(UnityEngine.Random.Range(0, playerStatsDict.Count));
        StartTurn(randomPlayerId);
    }

    public void StartTurn(int playerId)
    {
        if (!PhotonNetwork.IsMasterClient) return;

        currentTurnPlayerId = playerId;
        turnStartTime = PhotonNetwork.Time;
        isTurnActive = true;

        photonView.RPC(nameof(RPC_StartTurn), RpcTarget.All, playerId, turnStartTime);
        OnTurnStarted?.Invoke(playerId);
    }

    [PunRPC]
    private void RPC_StartTurn(int playerId, double startTime)
    {
        currentTurnPlayerId = playerId;
        turnStartTime = startTime;
        isTurnActive = true;

        DiceRollWindow.Instance.TurnChangeWindow(playerId);
        CellsManager.Instance.ShowBranchButtons(playerId);
    }

    public void RequestEndTurn()
    {
        if (PhotonNetwork.IsMasterClient) EndTurnInternal();
        else photonView.RPC(nameof(RPC_RequestEndTurn), RpcTarget.MasterClient);
    }

    [PunRPC]
    private void RPC_RequestEndTurn() => EndTurnInternal();

    private void EndTurnInternal()
    {
        if (!isTurnActive) return;

        isTurnActive = false;

        if (!PhotonNetwork.IsMasterClient) return;

        int nextPlayerId = GetNextPlayerId(currentTurnPlayerId);
        StartTurn(nextPlayerId);
    }

    private int GetNextPlayerId(int currentId)
    {
        var keys = playerStatsDict.Keys.ToList();
        int idx = (keys.IndexOf(currentId) + 1) % keys.Count;
        return keys[idx];
    }

    #endregion

    #region Dice

    public void RequestRollDice(int playerId)
    {
        photonView.RPC(nameof(RPC_RequestRollDice), RpcTarget.MasterClient, playerId);
    }

    [PunRPC]
    private void RPC_RequestRollDice(int playerId)
    {
        if (!PhotonNetwork.IsMasterClient) return;

        int first = UnityEngine.Random.Range(1, 7);
        int second = UnityEngine.Random.Range(1, 7);

        photonView.RPC(nameof(RPC_SetDiceResult), RpcTarget.AllBuffered, first, second, playerId);
    }

    [PunRPC]
    private void RPC_SetDiceResult(int first, int second, int playerId)
    {
        diceManager.StartDiceRollWithResult(first, second, playerId);
    }

    #endregion

    #region Branch Management

    public void RequestBuyBranch(int companyId) =>
        photonView.RPC(nameof(RPC_BuyBranchRequest), RpcTarget.MasterClient, companyId, PhotonNetwork.LocalPlayer.ActorNumber);

    public void RequestSellBranch(int companyId) =>
        photonView.RPC(nameof(RPC_SellBranchRequest), RpcTarget.MasterClient, companyId, PhotonNetwork.LocalPlayer.ActorNumber);

    [PunRPC]
    private void RPC_BuyBranchRequest(int companyId, int playerId)
    {
        if (!PhotonNetwork.IsMasterClient) return;
        var company = CompanyDatabase.Instance.GetCompanyById(companyId);
        var player = GameManager.Instance.GetPlayerById(playerId);

        if (company == null || company.OwnerId != playerId || company.RentLevel >= 5) return;
        if (!Bank.Instance.hasEnoughMoney(player, company.CompanyData.branchPrice)) return;

        company.RentLevel++;
        Bank.Instance.RemoveMoney(player, company.CompanyData.branchPrice);
        photonView.RPC(nameof(RPC_UpdateBranchUI), RpcTarget.All, playerId, companyId, company.RentLevel);
        photonView.RPC(nameof(RPC_HideButtons), RpcTarget.All, playerId, companyId);
    }

    [PunRPC]
    private void RPC_SellBranchRequest(int companyId, int playerId)
    {
        if (!PhotonNetwork.IsMasterClient) return;
        var company = CompanyDatabase.Instance.GetCompanyById(companyId);
        var player = GameManager.Instance.GetPlayerById(playerId);

        if (company == null || company.OwnerId != playerId || company.RentLevel <= 0) return;

        company.RentLevel--;
        Bank.Instance.AddMoney(player, company.CompanyData.branchPrice);
        photonView.RPC(nameof(RPC_UpdateBranchUI), RpcTarget.All, playerId, companyId, company.RentLevel);
        photonView.RPC(nameof(RPC_HideSellButtons), RpcTarget.All, playerId, companyId);
    }

    [PunRPC]
    private void RPC_UpdateBranchUI(int playerId, int companyId, int newLevel)
    {
        var company = CompanyDatabase.Instance.GetCompanyById(companyId);
        if (company == null) return;

        var uiCompany = CellsManager.Instance.GetCompanyUI(companyId);
        if (uiCompany == null) return;

        uiCompany.UpdateBranchStars(newLevel);
        uiCompany.SetRentText(company.CompanyData.rent[newLevel]);
    }

    [PunRPC]
    private void RPC_HideButtons(int playerId, int companyId)
    {
        var company = CompanyDatabase.Instance.GetCompanyById(companyId);
        if (company == null) return;
        CellsManager.Instance.HideAllBranchButtonsByGroup(playerId, company.Group);
    }

    [PunRPC]
    private void RPC_HideSellButtons(int playerId, int companyId)
    {
        var company = CompanyDatabase.Instance.GetCompanyById(companyId);
        if (company == null) return;

        var ui = CellsManager.Instance.GetCompanyUI(companyId);
        if (ui == null) return;

        if (company.RentLevel == 0)
            ui.HideAllBranchButtons();
        else if (company.RentLevel == 4)
            ui.ShowBuySellButtons();
    }

    #endregion
}
