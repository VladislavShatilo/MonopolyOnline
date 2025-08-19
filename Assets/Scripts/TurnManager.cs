using Photon.Pun;
using Photon.Pun.Demo.PunBasics;
using System;
using System.Collections.Generic;
using UnityEngine;

public class TurnManager : MonoBehaviourPunCallbacks
{
    public static TurnManager Instance { get; private set; }

    public float turnDuration = 90f;
    [SerializeField] private DiceManagerPhoton diceManager;

    private double turnStartTime; // время старта хода (PhotonNetwork.Time)
    private int currentTurnPlayerId;

    private bool isTurnActive = false;

    // UI игроков, ключ — ActorNumber.ToString()
    private Dictionary<int, UIPlayerStats> playerStatsDict = new Dictionary<int, UIPlayerStats>();

    public int CurrentTurnPlayerId => currentTurnPlayerId;

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
        float timeLeft = Mathf.Clamp((float)(turnDuration - elapsed), 0f, turnDuration);

        UpdateTurnTimerOnAllClients(timeLeft);

        if (timeLeft <= 0f)
        {
            EndTurn();
        }
    }

    public void RegisterPlayerUI(int playerId, UIPlayerStats uiPlayerStats)
    {
        if (!playerStatsDict.ContainsKey(playerId))
        {
            playerStatsDict.Add(playerId, uiPlayerStats);
        }
    }

    public void StartRandomTurn()
    {
        if (!PhotonNetwork.IsMasterClient || playerStatsDict.Count == 0)
            return;

        var keys = new List<int>(playerStatsDict.Keys);
        int randomIndex = UnityEngine.Random.Range(0, keys.Count);
        int randomPlayerId = keys[randomIndex];

        StartTurn(randomPlayerId);
    }

    public void RequestRollDice(int requestingPlayerId)
    {
        // Любой игрок вызывает бросок — отправляем запрос мастеру
        photonView.RPC(nameof(RPC_RequestSetNumbersDice), RpcTarget.MasterClient, requestingPlayerId);
    }

    [PunRPC]
    private void RPC_RequestSetNumbersDice(int requestingPlayerId)
    {
        if (!PhotonNetwork.IsMasterClient) return;

        // Генерируем числа кубиков
        int first = UnityEngine.Random.Range(1, 7);
        int second = UnityEngine.Random.Range(1, 7);

        // Запускаем кубики у всех клиентов с этими числами
        photonView.RPC(nameof(RPC_RequestSetNumbersDice), RpcTarget.AllBuffered, first, second, requestingPlayerId);
    }

    [PunRPC]
    private void RPC_RequestSetNumbersDice(int first, int second, int requestingPlayerId)
    {
        diceManager.StartDiceRollWithResult(first, second, requestingPlayerId);
    }

    public void StartTurn(int playerId)
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        currentTurnPlayerId = playerId;
        turnStartTime = PhotonNetwork.Time;
        isTurnActive = true;

        photonView.RPC(nameof(RPC_StartTurn), RpcTarget.All, playerId, turnStartTime);
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

    public void RequestSellBranch(int companyId)
    {
        // локальный игрок отправляет запрос мастеру
        photonView.RPC(nameof(RPC_RequestSellBranch), RpcTarget.MasterClient, companyId, PhotonNetwork.LocalPlayer.ActorNumber);
    }

    [PunRPC]
    private void RPC_RequestSellBranch(int companyId, int requestingPlayerId)
    {
        if (!PhotonNetwork.IsMasterClient) return;

        var company = CompanyDatabase.Instance.GetCompanyById(companyId);
        if (company == null) return;

        // Проверка условий (владелец, деньги, лимит филиалов)
        if (company.OwnerId == requestingPlayerId && company.RentLevel > 0)
        {
            int sellPrice = company.CompanyBranchData.branchPrice;
            PlayerData player = GameManager.Instance.GetPlayerById(requestingPlayerId);

            company.RentLevel--;

            photonView.RPC(nameof(RPC_OnSellBranchChange), RpcTarget.All,requestingPlayerId, companyId, company.RentLevel);
            photonView.RPC(nameof(RPC_OnHideSellsButtons), RpcTarget.All, requestingPlayerId, companyId);
        }
    }

    public void RequestBuyBranch(int companyId)
    {
        // локальный игрок отправляет запрос мастеру
        photonView.RPC(nameof(RPC_RequestBuyBranch), RpcTarget.MasterClient, companyId, PhotonNetwork.LocalPlayer.ActorNumber);
    }

    [PunRPC]
    private void RPC_RequestBuyBranch(int companyId, int requestingPlayerId)
    {
        if (!PhotonNetwork.IsMasterClient) return;

        var company = CompanyDatabase.Instance.GetCompanyById(companyId);
        if (company == null) return;

        // Проверка условий (владелец, деньги, лимит филиалов)
        if (company.OwnerId == requestingPlayerId && company.RentLevel < 5)
        {
            int price = company.CompanyBranchData.branchPrice;
            PlayerData player = GameManager.Instance.GetPlayerById(requestingPlayerId);
            if (!Bank.Instance.hasEnoughMoney(player, price)) return;

            company.RentLevel++;

            Debug.Log($"[Master] Игрок {requestingPlayerId} купил филиал {companyId}, новый уровень = {company.RentLevel}");

            photonView.RPC(nameof(RPC_OnBuyBranchChange), RpcTarget.All, requestingPlayerId, companyId, company.RentLevel);
            photonView.RPC(nameof(RPC_OnHideButtons), RpcTarget.All, requestingPlayerId, companyId);
        }
    }

    [PunRPC]
    private void RPC_OnHideSellsButtons(int playerID, int companyId)
    {
        var company = CompanyDatabase.Instance.GetCompanyById(companyId);
        if (company != null)
        {
            if (CellsManager.Instance.GetCompanyUI(companyId)?.TryGetComponent(out UICompanyCell uiCell) == true)
            {
                int newCompanyRentLevel = company.RentLevel;
                if (newCompanyRentLevel == 4)
                {
                    uiCell.ShowBuySellButtons();
                }
                if(newCompanyRentLevel == 0)
                {
                    uiCell.HideAllBranchButtons();

                }
            }
        }
    }

    [PunRPC]
    private void RPC_OnHideButtons(int playerID, int companyId)
    {
        var company = CompanyDatabase.Instance.GetCompanyById(companyId);
        if (company != null)
        {
            CellsManager.Instance.HideAllBranchButtonsByGroup(playerID, company.CompanyBranchData.group);
        }
    }

    [PunRPC]
    private void RPC_OnBuyBranchChange(int playerID,int companyId, int newLevel)
    {
        var company = CompanyDatabase.Instance.GetCompanyById(companyId);
        if (company != null)
        {
            var uiCompany = CellsManager.Instance.GetCompanyUI(companyId);
            if (uiCompany != null)
            {
                int price = company.CompanyBranchData.branchPrice;
                PlayerData player = GameManager.Instance.GetPlayerById(playerID);

                Bank.Instance.RemoveMoney(player, price);

                company.RentLevel = newLevel;
                uiCompany.UpdateBranchStars(newLevel);
                uiCompany.SetRentText(company.CompanyBranchData.rent[newLevel]);
            }
        }
    }
    [PunRPC]
    private void RPC_OnSellBranchChange(int playerID, int companyId, int newLevel)
    {
        var company = CompanyDatabase.Instance.GetCompanyById(companyId);
        if (company != null)
        {
            var uiCompany = CellsManager.Instance.GetCompanyUI(companyId);
            if (uiCompany != null)
            {
                int price = company.CompanyBranchData.branchPrice;
                PlayerData player = GameManager.Instance.GetPlayerById(playerID);

                Bank.Instance.AddMoney(player, price);

                company.RentLevel = newLevel;
                uiCompany.UpdateBranchStars(newLevel);
                uiCompany.SetRentText(company.CompanyBranchData.rent[newLevel]);
            }
        }
    }
    private void UpdateTurnTimerOnAllClients(float timeLeft)
    {
        foreach (var kvp in playerStatsDict)
        {
            if (kvp.Key == currentTurnPlayerId)
            {
                kvp.Value.SetTurnActive(true);
                kvp.Value.UpdateTurnTimer(timeLeft);
            }
            else
            {
                kvp.Value.SetTurnActive(false);
            }
        }
    }

    private void EndTurn()
    {
        if (!isTurnActive) return;

        isTurnActive = false;
        if (PhotonNetwork.IsMasterClient)
        {
            int nextPlayerId = GetNextPlayerId(currentTurnPlayerId);
            StartTurn(nextPlayerId);
            Debug.Log(nextPlayerId);
        }
    }

    private int GetNextPlayerId(int currentId)
    {
        var keys = new List<int>(playerStatsDict.Keys);
        int idx = keys.IndexOf(currentId);
        idx = (idx + 1) % keys.Count;
        return keys[idx];
    }

    public void RequestEndTurn()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            EndTurn();
        }
        else
        {
            photonView.RPC(nameof(RPC_RequestEndTurn), RpcTarget.MasterClient);
        }
    }

    [PunRPC]
    private void RPC_RequestEndTurn()
    {
        EndTurn();
    }
}