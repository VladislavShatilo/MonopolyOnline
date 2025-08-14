using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;

public class CompanyManager : MonoBehaviourPun
{
    public static CompanyManager Instance { get; private set; }

    /// <summary> Компании по индексу клетки. </summary>
    private readonly Dictionary<int, CellData> cells = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
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

    #region Покупка компаний

    /// <summary> Отправляет предложение игроку купить компанию. </summary>
    public void CompanyHandle(int cellIndex, int playerId)
    {
        if (!cells[cellIndex].companyData.isBought)
        {
            photonView.RPC(nameof(RPC_ShowPurchaseOffer), PhotonNetwork.CurrentRoom.GetPlayer(playerId), cellIndex);
        }
        else if(cells[cellIndex].companyData.isBought)
        {
            if(cells[cellIndex].companyData.ownerID == playerId)
            {
                photonView.RPC(nameof(RPC_ShowBranchOffer), PhotonNetwork.CurrentRoom.GetPlayer(playerId), cellIndex);
            }
            else
            {
                photonView.RPC(nameof(RPC_ShowRentOffer), PhotonNetwork.CurrentRoom.GetPlayer(playerId), cellIndex);

            }
        }
    }

  
    /// <summary> Мастер показывает игроку окно филиала. </summary>
    [PunRPC]
    private void RPC_ShowBranchOffer(int cellIndex)
    {
        UIBuyWindow.Instance.ShowBuyWindow(cellIndex, cells[cellIndex].companyData);
    }

   
    /// <summary> Мастер показывает игроку окно покупки. </summary>
    [PunRPC]
    private void RPC_ShowPurchaseOffer(int cellIndex)
    {
        if (!ValidateCompanyAvailable(cellIndex, out var cell)) return;
        UIBuyWindow.Instance.ShowBuyWindow(cellIndex, cell.companyData);
    }
    /// <summary> Локальная попытка купить компанию. </summary>
    public void TryBuyCompany(int cellIndex)
    {
        if (!ValidateCompanyExists(cellIndex, out var cell)) return;
        if (cell.companyData.isBought)
        {
            UnityEngine.Debug.Log("Компания уже куплена");
            return;
        }

        int playerId = PhotonNetwork.LocalPlayer.ActorNumber;
        photonView.RPC(nameof(RPC_RequestBuyCompany), RpcTarget.MasterClient, cellIndex, playerId);
    }
  

    /// <summary> Мастер обрабатывает запрос на покупку. </summary>
    [PunRPC]
    private void RPC_RequestBuyCompany(int cellIndex, int buyerId)
    {
        if (!ValidateCompanyAvailable(cellIndex, out var cell)) return;

        var buyer = GameManager.Instance.GetPlayerById(buyerId);
        int price = cell.companyData.price[0];

        if (!Bank.Instance.hasEnoughMoney(buyer, price))
        {
            Debug.Log("Недостаточно денег для покупки");
            return;
        }

        cell.companyData.isBought = true;
        cell.companyData.ownerID = buyerId;

        Debug.Log($"Игрок {buyerId} купил компанию {cell.companyData.name}");

        photonView.RPC(nameof(RPC_ConfirmPurchase), RpcTarget.AllBuffered, cellIndex, buyerId);
    }

  
    /// <summary> Обновляет состояние компании у всех клиентов. </summary>
    [PunRPC]
    private void RPC_ConfirmPurchase(int cellIndex, int ownerId)
    {
        if (!ValidateCompanyExists(cellIndex, out var cell)) return;

        cell.companyData.isBought = true;
        cell.companyData.ownerID = ownerId;
        var buyer = GameManager.Instance.GetPlayerById(ownerId);
        int price = cell.companyData.price[0];
        Bank.Instance.RemoveMoney(buyer, price);
        var owner = GameManager.Instance.GetPlayerById(ownerId);
        if (CellsManager.Instance.GetCellByIndex(cellIndex)?.TryGetComponent(out UICompanyCell uiCell) == true)
            uiCell.UpdateUI(cell, owner);
        UIBuyWindow.Instance.HideWindow();
        TurnManager.Instance.RequestEndTurn();

    }

    #endregion

    #region Аренда

    /// <summary> Мастер показывает игроку окно аренды. </summary>
    [PunRPC]
    private void RPC_ShowRentOffer(int cellIndex)
    {
        UIBuyWindow.Instance.ShowBuyWindow(cellIndex, cells[cellIndex].companyData);
    }

    /// <summary> Локальная попытка оплатить аренду. </summary>
    public void TryPayRent(int cellIndex)
    {

        int playerId = PhotonNetwork.LocalPlayer.ActorNumber;
        photonView.RPC(nameof(RPC_RequestRent), RpcTarget.MasterClient, cellIndex, playerId);
    }

    /// <summary> Мастер обрабатывает запрос на аренду. </summary>

    [PunRPC]
    private void RPC_RequestRent(int cellIndex, int renterID)
    {

        var renter = GameManager.Instance.GetPlayerById(renterID);
        int price = cells[cellIndex].companyData.rent[0];

        if (!Bank.Instance.hasEnoughMoney(renter, price))
        {
            Debug.Log("Недостаточно денег для покупки");
            return;
        }

        Debug.Log($"Игрок {renterID} купил компанию {cells[cellIndex].companyData.name}");

        photonView.RPC(nameof(RPC_ConfirmRent), RpcTarget.AllBuffered, cellIndex, renterID);
    }
    /// <summary> Подтвердение платы за аренду. </summary>
    [PunRPC]
    private void RPC_ConfirmRent(int cellIndex, int renterId)
    {


        var renter = GameManager.Instance.GetPlayerById(renterId);
        int price = cells[cellIndex].companyData.rent[0];
        Bank.Instance.RemoveMoney(renter, price);
        UIPayRent.Instance.HideWindow();
        TurnManager.Instance.RequestEndTurn();

    }


    #endregion

    #region Helpers

    public CompanyData GetCompany(int cellIndex) =>
        cells.TryGetValue(cellIndex, out var cell) ? cell.companyData : null;

    /// <summary> Проверка, что клетка существует. </summary>
    private bool ValidateCompanyExists(int cellIndex, out CellData cell)
    {
        if (!cells.TryGetValue(cellIndex, out cell))
        {
            Debug.LogError($"Клетка с индексом {cellIndex} не найдена");
            return false;
        }
        return true;
    }

    /// <summary> Проверка, что клетка — это доступная для покупки компания. </summary>
    private bool ValidateCompanyAvailable(int cellIndex, out CellData cell)
    {
        if (!ValidateCompanyExists(cellIndex, out cell)) return false;
        if (cell.cellType != CellType.Company || cell.companyData.isBought)
            return false;
        return true;
    }

    #endregion
}
