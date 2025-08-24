using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Менеджер компаний: обработка покупки, аренды, взаимодействие с UI через события.
/// </summary>
public class CompanyManager : MonoBehaviourPun
{
    public static CompanyManager Instance { get; private set; }

    private readonly Dictionary<CompanyType, ICellHandler> handlers = new();

    #region События

    // Событие: компания куплена (cellIndex, ownerId)
    public event System.Action<int, int> OnCompanyBought;

    // Событие: арендная плата произведена (cellIndex, payerId, ownerId, amount)
    public event System.Action<int, int, int, int> OnRentPaid;

    #endregion

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // Регистрируем обработчики клеток
        handlers[CompanyType.Company] = new DefaultCompanyHandler();
        handlers[CompanyType.FieldCompany] = new FieldCompanyHandler();
        handlers[CompanyType.DiceCompany] = new DiceCompanyHandler();
    }

    #region Работа с клеткой

    public void HandleCell(int cellIndex, int playerId)
    {
        var company = CompanyDatabase.Instance.GetCompanyById(cellIndex);
        if(company == null)
        {
            Debug.LogError($"Клетка с индексом {cellIndex} не найдена");
            return;
        }
       
        if (TryGetHandler(company.Type, out var handler))
        {
            handler.Handle(cellIndex, playerId);

        }
        else
        {
            Debug.LogWarning($"Нет обработчика для {company.Type}");
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
        if (!TryGetCompanyAndHandler(cellIndex, out var company, out var handler)) return;
        handler.ShowPurchaseUI(cellIndex);
    }

    public void TryBuyCompany(int cellIndex)
    {
        var company = CompanyDatabase.Instance.GetCompanyById(cellIndex);
        if (company == null || company.IsBought) return;

        int playerId = PhotonNetwork.LocalPlayer.ActorNumber;
        photonView.RPC(nameof(RPC_RequestBuyCompany), RpcTarget.MasterClient, cellIndex, playerId);
    }

    [PunRPC]
    private void RPC_RequestBuyCompany(int cellIndex, int buyerId)
    {
        if (!TryGetCompanyAndHandler(cellIndex, out var company, out var handler)) return;

        var buyer = GameManager.Instance.GetPlayerById(buyerId);
        int price = handler.GetPrice(cellIndex);

        if (!Bank.Instance.HasEnoughMoney(buyer, price))
        {
            Debug.Log("Недостаточно денег для покупки");
            return;
        }

        photonView.RPC(nameof(RPC_ConfirmPurchase), RpcTarget.AllBuffered, cellIndex, buyerId);
        TurnManager.Instance.RequestEndTurn();
    }

    [PunRPC]
    private void RPC_ConfirmPurchase(int cellIndex, int ownerId)
    {
        var company = CompanyDatabase.Instance.GetCompanyById(cellIndex);
        if (company == null || !TryGetHandler(cellIndex, out var handler)) return;

        company.IsBought = true;
        company.OwnerId = ownerId;

        int price = handler.GetPrice(cellIndex);
        var buyer = GameManager.Instance.GetPlayerById(ownerId);
        Bank.Instance.RemoveMoney(buyer, price);

     

        var cellUI = CellsManager.Instance.GetCellByIndex(cellIndex).GetComponent<UICompanyCell>();
        cellUI.HandleCompanyBought(cellIndex, ownerId);
        // Обновляем аренду для группы
        UpdateRent(company);
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
        if (!TryGetCompanyAndHandler(cellIndex, out var company, out var handler)) return;
        handler.ShowRentUI(cellIndex);
    }

    public void TryPayRent(int cellIndex)
    {
        int playerId = PhotonNetwork.LocalPlayer.ActorNumber;
        photonView.RPC(nameof(RPC_RequestRent), RpcTarget.MasterClient, cellIndex, playerId);
    }

    [PunRPC]
    private void RPC_RequestRent(int cellIndex, int renterId)
    {
        if (!TryGetCompanyAndHandler(cellIndex, out var company, out var handler)) return;

        var renter = GameManager.Instance.GetPlayerById(renterId);
        int rentPrice = handler.GetRent(cellIndex);

        if (!Bank.Instance.HasEnoughMoney(renter, rentPrice))
        {
            Debug.Log("Недостаточно денег для аренды");
            return;
        }

        photonView.RPC(nameof(RPC_ConfirmRent), RpcTarget.AllBuffered, cellIndex, renterId);
        TurnManager.Instance.RequestEndTurn();
    }

    [PunRPC]
    private void RPC_ConfirmRent(int cellIndex, int renterId)
    {
        if (!TryGetCompanyAndHandler(cellIndex, out var company, out var handler)) return;

        int rentPrice = handler.GetRent(cellIndex);
        int ownerId = handler.GetOwner(cellIndex);

        var renter = GameManager.Instance.GetPlayerById(renterId);
        var owner = GameManager.Instance.GetPlayerById(ownerId);

        Bank.Instance.TransferMoney(renter, owner, rentPrice);

        UIPayRent.Instance.HideWindow();
    }

    #endregion

    #region Вспомогательные методы

    public void EndTurnIfMine()
    {
        if (photonView.IsMine)
        {
            TurnManager.Instance.RequestEndTurn();
        }
    }
    private bool TryGetHandler(int cellIndex, out ICellHandler handler)
    {
        handler = null;
        var companyData = CompanyDatabase.Instance.GetCompanyById(cellIndex);
        if (companyData == null ) return false;

        return handlers.TryGetValue(companyData.Type, out handler);
    }

    private bool TryGetCompanyAndHandler(int cellIndex, out Company company, out ICellHandler handler)
    {
        handler = null;
        company = null;
        var companyData = CompanyDatabase.Instance.GetCompanyById(cellIndex);
        if (companyData == null) return false;
        if (!handlers.TryGetValue(companyData.Type, out handler)) return false;
        return true;
    }

    private void UpdateRent(Company company)
    {

        if (company.Type == CompanyType.FieldCompany || company.Type == CompanyType.DiceCompany)
        {
            foreach (var c in CompanyDatabase.Instance.Companies)
            {
                if (c.Type == company.Type && c.Group == company.Group && c.IsBought && c.OwnerId == company.OwnerId)
                {
                    int newRent = handlers[company.Type].GetRent(company.Id); 
                    if (company.Type == CompanyType.DiceCompany)
                    {
                       // newRent /= TurnManager.Instance.DiceSum;
                    }
                    if (CellsManager.Instance.GetCellByIndex(c.Id)?.TryGetComponent(out UICompanyCell uiCell) == true)
                    {
                        uiCell.SetRentText(newRent);
                    }
                }
            }
        }
        else if (company.Type == CompanyType.Company)
        {
            int baseRent = handlers[company.Type].GetRent(company.Id);
            if (CellsManager.Instance.GetCellByIndex(company.Id)?.TryGetComponent(out UICompanyCell uiCell) == true)
            {
                uiCell.SetRentText(baseRent);
            }
        }

       
    }
    public bool PlayerOwnsWholeGroup(CompanyGroup group, int playerId)
    {
        foreach (var company in CompanyDatabase.Instance.Companies)
        {
            if (company.Group == group)
            {
                if (!company.IsBought || company.OwnerId != playerId)
                    return false;
            }
        }
        return true;
    }

    public bool TryGetHandler(CompanyType type, out ICellHandler handler) =>
        handlers.TryGetValue(type, out handler);

    #endregion
}
