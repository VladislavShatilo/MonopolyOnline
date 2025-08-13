//using Photon.Pun;
//using System.Collections.Generic;
//using UnityEngine;

//public class CompanyManager : MonoBehaviourPun
//{
//    public static CompanyManager Instance;

//    // Словарь компаний по индексу клетки
//    private Dictionary<int, CellData> cells = new Dictionary<int, CellData>();

//    private void Awake()
//    {
//        if (Instance != null && Instance != this) Destroy(gameObject);
//        else Instance = this;
//    }

//    // Инициализация компаний из BoardConfig
//    public void InitializeCompanies(List<CellData> cells)
//    {
//        this.cells.Clear();
//        foreach (var cell in cells)
//        {
//             this.cells[cell.index] = cell;
//        }
//    }

//    // Клиент делает запрос купить компанию (вызывается локально)
//    public void TryBuyCompany(int cellIndex)
//    {
//        int playerId = PhotonNetwork.LocalPlayer.ActorNumber;
        
//        if (!cells.ContainsKey(cellIndex))
//        {
//            Debug.LogError("Компания не найдена");
//            return;
//        }

//        var cell = cells[cellIndex];
//        if(cell.companyData == null)
//        {
//            return;
//        }
//        if (cell.companyData.isBought)
//        {
//            Debug.Log("Компания уже куплена");
//            return;
//        }

//        // Отправляем RPC мастеру с запросом купить
//        photonView.RPC(nameof(RPC_RequestBuyCompany), RpcTarget.MasterClient, cellIndex, playerId);
//    }

//    // Мастер получает запрос на покупку
//    [PunRPC]
//    private void RPC_RequestBuyCompany(int cellIndex, int buyerId)
//    {
//        if (!cells.ContainsKey(cellIndex)) return;
//        var company = cells[cellIndex];

//        if (company.isBought)
//        {
//            Debug.Log("Компания уже куплена");
//            return;
//        }

//        int price = company.price[0];
//        PlayerData buyerPlayerData = GameManager.Instance.GetPlayerById(buyerId);

//        // Проверяем баланс через Bank на мастере
//        if (!Bank.Instance.RemoveMoney(buyerPlayerData, price))
//        {
//            Debug.Log("Недостаточно денег у игрока для покупки");
//            // Можно отправить ответ клиенту, что покупка не удалась
//            return;
//        }

//        // Обновляем состояние компании
//        company.isBought = true;
//        company.ownerID = buyerId;

//        Debug.Log($"Игрок {buyerId} купил компанию {company.name}");

//        // Синхронизируем покупку всем клиентам
//        photonView.RPC(nameof(RPC_ConfirmPurchase), RpcTarget.AllBuffered, cellIndex, buyerId);

//        // Можно вызвать событие для UI, например:
//        //OnCompanyPurchased?.Invoke(company);
//    }

//    // Всем клиентам обновляем состояние компании
//    [PunRPC]
//    private void RPC_ConfirmPurchase(int cellIndex, int ownerId)
//    {
//        if (!cells.ContainsKey(cellIndex)) return;

//        var company = cells[cellIndex];
//        company.isBought = true;
//        company.ownerID = ownerId;

//        // Обновляем UI, если нужно
//        // Например, обновить цвет клетки или владельца
//    }

//    // Обработка аренды — вызывается когда игрок встал на клетку компании
//    public void HandleRentPayment(int cellIndex, int playerId)
//    {
//        if (!cells.ContainsKey(cellIndex)) return;

//        var company = cells[cellIndex];
//        if (!company.isBought) return;
//        if (company.ownerID == playerId) return; // свой не платит5

//        int rentAmount = company.rent[0]; // можно расширить логику

//        // Клиент отправляет запрос на оплату аренды мастеру
//        photonView.RPC(nameof(RPC_RequestPayRent), RpcTarget.MasterClient, cellIndex, playerId, company.ownerID, rentAmount);
//    }

//    [PunRPC]
//    private void RPC_RequestPayRent(int cellIndex, int payerId, int ownerId, int rentAmount)
//    {
//        PlayerData payerPlayerData = GameManager.Instance.GetPlayerById(payerId);
//        PlayerData ownerPlayerData = GameManager.Instance.GetPlayerById(ownerId);

//        // Проверяем баланс и переводим деньги через Bank на мастере
//        if (!Bank.Instance.TransferMoney(payerPlayerData, ownerPlayerData, rentAmount))
//        {
//            Debug.Log($"Игрок {payerId} не может оплатить аренду {rentAmount}");
//            // Тут можно обработать банкротство
//            return;
//        }

//        Debug.Log($"Игрок {payerId} оплатил аренду {rentAmount} игроку {ownerId}");
//    }

//    // Получить данные компании по индексу
//    public CompanyData GetCompany(int cellIndex)
//    {
//        cells.TryGetValue(cellIndex, out var company);
//        return company;
//    }
//}
