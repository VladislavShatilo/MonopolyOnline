using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using UnityEngine;

public class MortgageManager : MonoBehaviourPun
{
    public static MortgageManager Instance { get; private set; }
    private const int MORTGAGE_TURNS_COUNTS = 7;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void OnEnable()
    {
        EventBus.Subscribe<TurnStartEvent>(OnTurnStart);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<TurnStartEvent>(OnTurnStart);
    }

    private void OnTurnStart(TurnStartEvent e)
    {
        if (CellsManager.Instance == null)
        {
            Debug.LogError("CellsManager.Instance == null!");
            return;
        }

        if (CompanyManager.Instance == null)
        {
            Debug.LogError("CompanyManager.Instance == null!");
            return;
        }

        foreach (var company in CompanyDatabase.Instance.Companies)
        {
            if (company.CompanyData == null && company.DiceCompanyData == null && company.FieldCompanyData == null)
            {
                Debug.LogWarning($"CompanyData == null для компании {company.Id}");
                continue;
            }

            var ui = CellsManager.Instance.GetCompanyUI(company.Id);
            if (ui == null)
            {
                Debug.LogWarning($"UICompanyCell == null для компании {company.Id}");
                continue;
            }

            bool isMyTurn = e.PlayerId == PhotonNetwork.LocalPlayer.ActorNumber;
            bool ownsGroup = false;
            if (company.Type == CompanyType.Company)
            {
                ownsGroup = CompanyManager.Instance.PlayerOwnsWholeGroup(company.CompanyData.group, e.PlayerId);

            }
            else if (company.Type == CompanyType.DiceCompany || company.Type == CompanyType.FieldCompany)
            {
                ownsGroup = false;
            }
            
            if (isMyTurn && company.IsBought && company.OwnerId == e.PlayerId && ! ownsGroup)
            {
                if (!company.IsMortgaged)
                {
                    ui.ShowMortgageButton();
                }
                else
                {
                    ui.ShowBuyoutButton();
                }
            }
            else if (isMyTurn && company.IsBought && company.OwnerId == e.PlayerId && ownsGroup)
            {
               BranchManager.Instance.ShowBranchButtonsForLevel(ui, company.RentLevel);
            }
            else
            {
                ui.HideAllBranchButtons();
                ui.HideAllButtnos();
            }
        }
    }

    public void RequestMortgageCompany(int companyId)
    {
        photonView.RPC(nameof(RPC_MortgageCompany), RpcTarget.MasterClient, companyId, PhotonNetwork.LocalPlayer.ActorNumber);
    }

    [PunRPC]
    private void RPC_MortgageCompany(int companyId, int playerId)
    {
        if (!PhotonNetwork.IsMasterClient) return;
        photonView.RPC(nameof(RPC_UpdateMortgageUI), RpcTarget.All, playerId, companyId, true);
    }

    [PunRPC]
    private void RPC_UpdateMortgageUI(int playerId, int companyId, bool isMortgage)
    {
        var company = CompanyDatabase.Instance.GetCompanyById(companyId);
        var player = GameManager.Instance.GetPlayerById(playerId);

        if (company.OwnerId != playerId) return;
        var ui = CellsManager.Instance.GetCompanyUI(company.Id);

        if (isMortgage)
        {
            company.IsMortgaged = true;
            company.MortgageTurnsLeft = MORTGAGE_TURNS_COUNTS;
            Debug.Log(company.MortgagePrice);
            Bank.Instance.AddMoney(player, company.MortgagePrice);
            ui.SetTurnsText(MORTGAGE_TURNS_COUNTS);
            ui.MortgageUI();
        }
        else
        {
            company.IsMortgaged = false;
            company.MortgageTurnsLeft = 0;
            Bank.Instance.RemoveMoney(player, company.BuyoutPrice);
            ui.BuyoutUI();
        }

        EventBus.Publish(new CompanyMortgagedEvent(playerId, companyId));
    }

    public void RequestBuyoutCompany(int companyId)
    {
        photonView.RPC(nameof(RPC_BuyBackCompany), RpcTarget.MasterClient, PhotonNetwork.LocalPlayer.ActorNumber, companyId);
    }

    [PunRPC]
    private void RPC_BuyBackCompany(int playerId, int companyId)
    {
        if (!PhotonNetwork.IsMasterClient) return;
        photonView.RPC(nameof(RPC_UpdateMortgageUI), RpcTarget.All, playerId, companyId, false);
    }

    // Метод, который вызывается в конце каждого хода всех игроков
    public void TickMortgageTurnsRequest(int playerID)
    {
        photonView.RPC(nameof(RPC_TickMortgageRequest), RpcTarget.MasterClient, playerID);
    }

    [PunRPC]
    private void RPC_TickMortgageRequest(int playerID)
    {
        if (!PhotonNetwork.IsMasterClient) return;
        photonView.RPC(nameof(RPC_TickMortgageUI), RpcTarget.All, playerID);
    }

    [PunRPC]
    private void RPC_TickMortgageUI(int playerID)
    {
        foreach (var company in CompanyDatabase.Instance.Companies)
        {
            if (company.OwnerId == playerID && company.IsMortgaged)
            {
                company.MortgageTurnsLeft--;
                var ui = CellsManager.Instance.GetCompanyUI(company.Id);
                ui.SetTurnsText(company.MortgageTurnsLeft);
                if (company.MortgageTurnsLeft <= 0)
                {
                    company.IsMortgaged = false;
                    company.OwnerId = -1;
                    company.IsBought = false;
                    ui.BuyoutUI();
                    EventBus.Publish(new CompanyFreedFromMortgageEvent(company.Id));
                }
            }
        }
    }
}

public class CompanyMortgagedEvent
{
    public int PlayerId { get; }
    public int CompanyId { get; }

    public CompanyMortgagedEvent(int playerId, int companyId)
    {
        PlayerId = playerId;
        CompanyId = companyId;
    }
}

public class CompanyBoughtBackEvent
{
    public int PlayerId { get; }
    public int CompanyId { get; }

    public CompanyBoughtBackEvent(int playerId, int companyId)
    {
        PlayerId = playerId;
        CompanyId = companyId;
    }
}

public class CompanyFreedFromMortgageEvent
{
    public int CompanyId { get; }

    public CompanyFreedFromMortgageEvent(int companyId)
    {
        CompanyId = companyId;
    }
}