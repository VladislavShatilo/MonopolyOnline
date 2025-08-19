using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;

public class CompanyManager : MonoBehaviourPun
{
    public static CompanyManager Instance { get; private set; }

    /// <summary> Компании по индексу клетки. </summary>
    private readonly Dictionary<int, CellData> cells = new();
    private readonly Dictionary<int, Company> companysWithBranches = new();

    /// <summary> Обработчики по типам клеток. </summary>
    private readonly Dictionary<CellType, ICellHandler> handlers = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // Регистрируем обработчики
        handlers[CellType.Company] = new DefaultCompanyHandler();
        handlers[CellType.FieldCompany] = new FieldCompanyHandler();
        handlers[CellType.DiceCompany] = new DiceCompanyHandler();
    }

    #region Initialization

    /// <summary> Загружает список компаний в словарь. </summary>
    public void InitializeCompanies(List<CellData> cells)
    {

        this.cells.Clear();
        CompanyDatabase companyDataBase = CompanyDatabase.Instance;
        for (int i = 0; i< cells.Count; i++)
        {
            if (cells[i].cellType == CellType.Company)
            {
                companysWithBranches.Add(i, companyDataBase.GetCompanyById(i));
                
            }
        }
        foreach (var cell in cells)
        {
            this.cells[cell.index] = cell;
         
        }
    }

    #endregion

    #region Обработка клетки

    public void HandleCell(int cellIndex, int playerId)
    {
        if (!cells.ContainsKey(cellIndex))
        {
            Debug.LogError($"Клетка с индексом {cellIndex} не найдена");
            return;
        }

        var cell = cells[cellIndex];
        if (handlers.TryGetValue(cell.cellType, out var handler))
        {
            handler.Handle(cellIndex, playerId);
        }
        else
        {
            Debug.LogWarning($"Нет обработчика для {cell.cellType}");
        }
    }

    #endregion

    #region Покупка компаний

    public void OfferPurchase(int cellIndex, int playerId)
    {
        photonView.RPC(nameof(RPC_ShowPurchaseOffer), PhotonNetwork.CurrentRoom.GetPlayer(playerId), cellIndex);
    }

    [PunRPC]
    private void RPC_ShowPurchaseOffer(int cellIndex)
    {
        if (!ValidateCompanyExists(cellIndex, out var cell)) return;

        if (handlers.TryGetValue(cell.cellType, out var handler))
        {
            handler.ShowPurchaseUI(cellIndex);
        }
    }

    public void TryBuyCompany(int cellIndex)
    {
        if (!ValidateCompanyExists(cellIndex, out var cell)) return;
        if (CompanyDatabase.Instance.GetCompanyById(cellIndex).IsBought) return;

        int playerId = PhotonNetwork.LocalPlayer.ActorNumber;
        photonView.RPC(nameof(RPC_RequestBuyCompany), RpcTarget.MasterClient, cellIndex, playerId);
    }

    [PunRPC]
    private void RPC_RequestBuyCompany(int cellIndex, int buyerId)
    {
        if (!ValidateCompanyExists(cellIndex, out var cell)) return;

        var buyer = GameManager.Instance.GetPlayerById(buyerId);
        int price = 0;
        if (handlers.TryGetValue(cell.cellType, out var handler))
        {
            price = handler.GetPrice(cellIndex);

        }
   
        if (!Bank.Instance.hasEnoughMoney(buyer, price))
        {
            Debug.Log("Недостаточно денег для покупки");
            return;
        }

        Debug.Log($"Игрок {buyerId} купил компанию {cell.cellName}");

        photonView.RPC(nameof(RPC_ConfirmPurchase), RpcTarget.AllBuffered, cellIndex, buyerId);
        TurnManager.Instance.RequestEndTurn();
    }

    [PunRPC]
    private void RPC_ConfirmPurchase(int cellIndex, int ownerId)
    {
        if (!ValidateCompanyExists(cellIndex, out var cell)) return;

        var company = CompanyDatabase.Instance.GetCompanyById(cellIndex);
        company.IsBought = true;
        company.OwnerId = ownerId;

        var buyer = GameManager.Instance.GetPlayerById(ownerId);
        int price =0,rent = 0;
        
        if (handlers.TryGetValue(cell.cellType, out var handler))
        {
            price = handler.GetPrice(cellIndex);
            rent = handler.GetRent(cellIndex);
        }
        Bank.Instance.RemoveMoney(buyer, price);

        if (CellsManager.Instance.GetCellByIndex(cellIndex)?.TryGetComponent(out UICompanyCell uiCell) == true)
        {
            uiCell.UpdateUI(cell, buyer);
            uiCell.SetRentText(rent);
        }

        UIBuyWindow.Instance.HideWindow();
    }

    #endregion

    #region Аренда

    public void OfferRent(int cellIndex, int playerId, int? customPrice = null)
    {
        photonView.RPC(nameof(RPC_ShowRentOffer), PhotonNetwork.CurrentRoom.GetPlayer(playerId), cellIndex, customPrice ?? -1);
    }

    [PunRPC]
    private void RPC_ShowRentOffer(int cellIndex, int rentPrice)
    {
        if (!ValidateCompanyExists(cellIndex, out var cell)) return;

        if (handlers.TryGetValue(cell.cellType, out var handler))
        {
            handler.ShowRentUI(cellIndex);
        }
    }

    public void TryPayRent(int cellIndex)
    {
        int playerId = PhotonNetwork.LocalPlayer.ActorNumber;
        photonView.RPC(nameof(RPC_RequestRent), RpcTarget.MasterClient, cellIndex, playerId);
    }

    [PunRPC]
    private void RPC_RequestRent(int cellIndex, int renterID)
    {
        if (!ValidateCompanyExists(cellIndex, out var cell)) return;

        var renter = GameManager.Instance.GetPlayerById(renterID);
        int rentPrice = 0;

        if (handlers.TryGetValue(cell.cellType, out var handler))
        {
            rentPrice = handler.GetRent(cellIndex);
        }
        if (!Bank.Instance.hasEnoughMoney(renter, rentPrice))
        {
            Debug.Log("Недостаточно денег для аренды");
            return;
        }

        photonView.RPC(nameof(RPC_ConfirmRent), RpcTarget.AllBuffered, cellIndex, renterID);
        TurnManager.Instance.RequestEndTurn();

    }

    [PunRPC]
    private void RPC_ConfirmRent(int cellIndex, int renterId)
    {
        if (!ValidateCompanyExists(cellIndex, out var cell)) return;

        var renter = GameManager.Instance.GetPlayerById(renterId);
        int rentPrice = 0;
        int ownerId = -1;
        if (handlers.TryGetValue(cell.cellType, out var handler))
        {
            rentPrice = handler.GetRent(cellIndex);
            ownerId = handler.GetOwner(cellIndex);
        }
        GameManager gameManager = GameManager.Instance;
        PlayerData renterPlayerData = gameManager.GetPlayerById(renterId);
        PlayerData ownerPlayerData = gameManager.GetPlayerById(ownerId);

        Bank.Instance.TransferMoney(renterPlayerData, ownerPlayerData, rentPrice);

        UIPayRent.Instance.HideWindow();
    }

    #endregion

    #region Helpers

    public CompanyData GetCompany(int cellIndex) =>
        cells.TryGetValue(cellIndex, out var cell) ? cell.companyData : null;

    private bool ValidateCompanyExists(int cellIndex, out CellData cell)
    {
        if (!cells.TryGetValue(cellIndex, out cell))
        {
            Debug.LogError($"Клетка с индексом {cellIndex} не найдена");
            return false;
        }
        return true;
    }
    public bool PlayerOwnsWholeGroup(CompanyGroup group, int playerId)
    {
        
        foreach (var company in companysWithBranches.Values) // companies — твой словарь компаний
        {
            if (company.CompanyBranchData.group == group)
            {
                if (!company.IsBought || company.OwnerId != playerId)
                    return false;
            }
        }
        return true;
    }
    public bool TryGetHandler(CellType type, out ICellHandler handler) =>
        handlers.TryGetValue(type, out handler);

    #endregion
}
