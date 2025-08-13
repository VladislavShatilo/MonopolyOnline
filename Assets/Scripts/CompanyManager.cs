using Photon.Pun;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CompanyManager : MonoBehaviourPun
{
    public static CompanyManager Instance;

    // Словарь компаний по индексу клетки
    private Dictionary<int, CellData> cells = new Dictionary<int, CellData>();

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;
    }

    // Инициализация компаний из BoardConfig
    public void InitializeCells(List<CellData> cells)
    {
        this.cells.Clear();
        foreach (var cell in cells)
        {
            this.cells[cell.index] = cell;
        }
    }
    public void OfferPurchaseToPlayer(int cellIndex, int playerId)
    {
        // Отправляем только этому игроку
        photonView.RPC(nameof(RPC_ShowPurchaseOffer), PhotonNetwork.CurrentRoom.GetPlayer(playerId), cellIndex);
    }

    // Клиент делает запрос купить компанию (вызывается локально)
    public void TryBuyCompany(int cellIndex)
    {
        int playerId = PhotonNetwork.LocalPlayer.ActorNumber;

        if (!cells.ContainsKey(cellIndex))
        {
            Debug.LogError("Компания не найдена");
            return;
        }

        var cell = cells[cellIndex];
       
        if (cell.cellType != CellType.Company)
        {
            return;
        }
        if (cell.companyData.isBought)
        {
            Debug.Log("Компания уже куплена");
            return;
        }

        // Отправляем RPC мастеру с запросом купить
        photonView.RPC(nameof(RPC_RequestBuyCompany), RpcTarget.MasterClient, cellIndex, playerId);
    }
    [PunRPC]
    private void RPC_ShowPurchaseOffer(int cellIndex)
    {
        if (!cells.ContainsKey(cellIndex))
            return;

        var companyCell = cells[cellIndex];

        // Проверка на тип клетки
        if (companyCell.cellType != CellType.Company || companyCell.companyData.isBought)
            return;

        // Показываем UI окно покупки
        UIBuyWindow.Instance.ShowBuyWindow(cellIndex, companyCell.companyData);
    }
    // Мастер получает запрос на покупку
    [PunRPC]
    private void RPC_RequestBuyCompany(int cellIndex, int buyerId)
    {
        if (!cells.ContainsKey(cellIndex))
        {
            return;
        }

        var companyCell = cells[cellIndex];
        if (companyCell.cellType != CellType.Company)
        {
            return;
        }

        if (companyCell.companyData.isBought)
        {
            Debug.Log("Компания уже куплена");
            return;
        }

        int price = companyCell.companyData.price[0];
        PlayerData buyerPlayerData = GameManager.Instance.GetPlayerById(buyerId);

        // Проверяем баланс через Bank на мастере
        if (!Bank.Instance.RemoveMoney(buyerPlayerData, price))
        {
            Debug.Log("Недостаточно денег у игрока для покупки");
            // Можно отправить ответ клиенту, что покупка не удалась
            return;
        }

        // Обновляем состояние компании
        companyCell.companyData.isBought = true;
        companyCell.companyData.ownerID = buyerId;

        Debug.Log($"Игрок {buyerId} купил компанию {companyCell.companyData.name}");

        // Синхронизируем покупку всем клиентам
        photonView.RPC(nameof(RPC_ConfirmPurchase), RpcTarget.AllBuffered, cellIndex, buyerId);

        // Можно вызвать событие для UI, например:
        //OnCompanyPurchased?.Invoke(company);
    }

    // Всем клиентам обновляем состояние компании
    [PunRPC]
    private void RPC_ConfirmPurchase(int cellIndex, int ownerId)
    {
        if (!cells.ContainsKey(cellIndex))
        {
            return;
        }

        var companyCell = cells[cellIndex];
        if (companyCell.cellType != CellType.Company)
        {
            return;
        }
        companyCell.companyData.isBought = true;
        companyCell.companyData.ownerID = ownerId;
        PlayerData owner = GameManager.Instance.GetPlayerById(ownerId);

        // Находим UI элемент этой компании
        UICompanyCell uiCell = CellsManager.Instance.GetCellByIndex(cellIndex).GetComponent<UICompanyCell>();
        if (uiCell != null)
        {
            uiCell.UpdateUI(companyCell, owner);
        }
        // Обновляем UI, если нужно
        // Например, обновить цвет клетки или владельца
    }

    // Обработка аренды — вызывается когда игрок встал на клетку компании
    public void HandleRentPayment(int cellIndex, int playerId)
    {
        if (!cells.ContainsKey(cellIndex)) return;

        var companyCell = cells[cellIndex];
        if (companyCell.cellType != CellType.Company)
        {
            return;
        }
        if (!companyCell.companyData.isBought)
        {
            return;
        }
        if (companyCell.companyData.ownerID == playerId)
        {
            return; // свой не платит5
            //реализовать улучшение
        }

        int rentAmount = companyCell.companyData.rent[0]; // можно расширить логику

        // Клиент отправляет запрос на оплату аренды мастеру
        photonView.RPC(nameof(RPC_RequestPayRent), RpcTarget.MasterClient, cellIndex, playerId, companyCell.companyData.ownerID, rentAmount);
    }

    [PunRPC]
    private void RPC_RequestPayRent(int cellIndex, int payerId, int ownerId, int rentAmount)
    {
        PlayerData payerPlayerData = GameManager.Instance.GetPlayerById(payerId);
        PlayerData ownerPlayerData = GameManager.Instance.GetPlayerById(ownerId);

        // Проверяем баланс и переводим деньги через Bank на мастере
        if (!Bank.Instance.TransferMoney(payerPlayerData, ownerPlayerData, rentAmount))
        {
            Debug.Log($"Игрок {payerId} не может оплатить аренду {rentAmount}");
            // Тут можно обработать банкротство
            return;
        }

        Debug.Log($"Игрок {payerId} оплатил аренду {rentAmount} игроку {ownerId}");
    }

    // Получить данные компании по индексу
    public CompanyData GetCompany(int cellIndex)
    {
          return cells[cellIndex].companyData;
    }
}
