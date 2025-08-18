using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;

public class CompanyManager : MonoBehaviourPun
{
    public static CompanyManager Instance { get; private set; }

    /// <summary> Компании по индексу клетки. </summary>
    private readonly Dictionary<int, CellData> cells = new();

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
        foreach (var cell in cells)
            this.cells[cell.index] = cell;
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
        int price = cell.companyData.price;

        if (!Bank.Instance.hasEnoughMoney(buyer, price))
        {
            Debug.Log("Недостаточно денег для покупки");
            return;
        }

        Debug.Log($"Игрок {buyerId} купил компанию {cell.companyData.name}");

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
        int price = cell.companyData.price;
        Bank.Instance.RemoveMoney(buyer, price);

        if (CellsManager.Instance.GetCellByIndex(cellIndex)?.TryGetComponent(out UICompanyCell uiCell) == true)
            uiCell.UpdateUI(cell, buyer);

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
        int price = rentPrice >= 0 ? rentPrice : cell.companyData.rent[0];
      //  UIPayRent.Instance.ShowRentWindow(cellIndex, (float)price);
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
        int price = cell.companyData.rent[0];

        if (!Bank.Instance.hasEnoughMoney(renter, price))
        {
            Debug.Log("Недостаточно денег для аренды");
            return;
        }

        photonView.RPC(nameof(RPC_ConfirmRent), RpcTarget.AllBuffered, cellIndex, renterID);
    }

    [PunRPC]
    private void RPC_ConfirmRent(int cellIndex, int renterId)
    {
        if (!ValidateCompanyExists(cellIndex, out var cell)) return;

        var renter = GameManager.Instance.GetPlayerById(renterId);
        int price = cell.companyData.rent[0];
        Bank.Instance.RemoveMoney(renter, price);

        UIPayRent.Instance.HideWindow();
        TurnManager.Instance.RequestEndTurn();
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

    public bool TryGetHandler(CellType type, out ICellHandler handler) =>
        handlers.TryGetValue(type, out handler);

    #endregion
}
