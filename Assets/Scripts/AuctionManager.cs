using Photon.Pun;
using Photon.Pun.Demo.PunBasics;
using Photon.Realtime;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Менеджер аукциона. Вся логика и состояние — на MasterClient.
/// Клиенты только показывают UI и отправляют решения (ставка/пас).
/// </summary>
public class AuctionManager : MonoBehaviourPunCallbacks
{
    public static AuctionManager Instance { get; private set; }

    // --- Состояние аукциона (только на мастере) ---
    private int starterId;
    private bool isAuctionActive;
    private int companyId;
    private int basePrice;                   // базовая цена компании
    private int currentPrice;                // текущая цена (последняя принятая ставка)
    private int currentBidderIndex;          // индекс в очереди bidders
    private int lastBidderActorNumber = -1;  // кто сделал последнюю ставку (-1 если ставок не было)

    // Очередь участников по ходу, начиная со следующего за инициатором
    private List<int> bidders = new List<int>();
    private HashSet<int> passed = new HashSet<int>();

    // Константы правил аукциона
    private const int FIRST_BID_INCREMENT = 100; // первая минимальная ставка = basePrice + 100
    private const float BID_DURATION = 10f; //  10 секунд на ход
    private double bidStartTime;
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }
    }

    #region Public API (вызывают игровые системы)

    /// <summary>
    /// Старт аукциона. Вызывать у мастера после отказа игрока от покупки компании.
    /// </summary>
    /// <param name="starterActorNumber">ActorNumber игрока, который отказался от покупки</param>
    /// <param name="companyId">ID компании на доске</param>
    /// <param name="companyBasePrice">Базовая цена компании</param>

    public void StartAuctionRequest(int starterActorNumber, int companyId, int companyBasePrice)
    {
        photonView.RPC(nameof(RPC_StartAuctionRequest), RpcTarget.MasterClient, starterActorNumber, companyId, companyBasePrice);

    }

    [PunRPC]
    private void RPC_StartAuctionRequest(int starterActorNumber, int companyId, int companyBasePrice)
    {
        if (!PhotonNetwork.IsMasterClient) return;


        photonView.RPC(nameof(RPC_UpdateAuction), RpcTarget.All, starterActorNumber, companyId, companyBasePrice);

        PromptCurrentBidder();

    }
    [PunRPC]
    public void RPC_UpdateAuction(int starterActorNumber, int companyId, int companyBasePrice)
    {
        bidders.Clear();
        passed.Clear();
        isAuctionActive = true;
        lastBidderActorNumber = -1;
        currentBidderIndex = 0;

        starterId = starterActorNumber;
        this.companyId = companyId;
        basePrice = companyBasePrice;
        currentPrice = basePrice;

        TurnManager.Instance.SetMode(TurnMode.Auction);
        passed.Add(starterActorNumber);

        bidders = BuildTurnOrderStartingAfter(starterActorNumber, passed);
        if (bidders.Count == 0)
        {
            EndAuction_NoWinner();
            return;
        }



        currentBidderIndex = 0;
    }

    /// <summary>
    /// Клиент (локальный игрок) отправляет ставку мастеру.
    /// </summary>
    /// 

    public void PlayActionRequest(int playerId)
    {
        photonView.RPC(nameof(RPC_PlayActionRequest), RpcTarget.MasterClient, playerId);

    }

    [PunRPC]
    public void RPC_PlayActionRequest(int playerId)
    {
        if (!PhotonNetwork.IsMasterClient || !isAuctionActive) return;
        int activeNotPassed = PhotonNetwork.PlayerList.Length - passed.Count;
        if (bidders.Count == 1 || activeNotPassed <= 1)
        {
            Debug.Log("[Auction] Only one active player left → auto-win");
            EndAuction_WithWinner(playerId, basePrice+ FIRST_BID_INCREMENT);
            return;
        }
        // Проверка: сейчас ли ход этого игрока?
        int currentActor = bidders[currentBidderIndex];
        if (currentActor != playerId)
        {
            Debug.LogWarning($"[Auction] Not bidder's turn. Actor {playerId}");
            return;
        }
        
        currentPrice = (lastBidderActorNumber == -1) ? basePrice + FIRST_BID_INCREMENT : currentPrice + FIRST_BID_INCREMENT;

        lastBidderActorNumber = playerId;

       
        photonView.RPC(nameof(RPC_UpdatePlayAction), RpcTarget.All, playerId, currentPrice);


        MoveToNextBidderAndPrompt();
    }
    [PunRPC]
    public void RPC_UpdatePlayAction(int playerId, int newPrice)
    {
        lastBidderActorNumber = playerId;
        currentPrice = newPrice;

    }

    /// <summary>
    /// Клиент (локальный игрок) отправляет пас мастеру.
    /// </summary>


    public void PassRequest(int playerId)
    {
        photonView.RPC(nameof(RPC_RequestPass), RpcTarget.MasterClient, playerId);
    }
    [PunRPC]
    private void RPC_RequestPass(int playerId)
    {
        if (!PhotonNetwork.IsMasterClient || !isAuctionActive) return;

        // Проверка: сейчас ли ход этого игрока?
        int currentActor = bidders[currentBidderIndex];
        if (currentActor != playerId)
        {
            Debug.LogWarning($"[Auction] Not bidder's turn to pass. Actor {playerId}");
            return;
        }
        photonView.RPC(nameof(RPC_UpdatePassAction), RpcTarget.All, playerId);

        // Условия завершения:
        // 1) Если никто ни разу не ставил и все пасовали → нет победителя.
        bool noBids = lastBidderActorNumber == -1;
        bool everyonePassed = bidders.All(b => passed.Contains(b));

        if (noBids && everyonePassed)
        {
            Debug.Log("[Auction] Все отказались, ставок не было → компания никому не достаётся.");
            EndAuction_NoWinner();
            return;
        }

        // 2) Если ставок уже были и остался один не пасовавший → он победитель.
        int activeNotPassed = bidders.Count - passed.Count;
        if (lastBidderActorNumber != -1 && activeNotPassed <= 1)
        {
            Debug.Log("lastBidderActorNumber != -1 && activeNotPassed <= 1");
            EndAuction_WithWinner(lastBidderActorNumber, currentPrice);
            return;
        }

        // Иначе — передаём ход следующему не пасовавшему
        MoveToNextBidderAndPrompt();
    }

    [PunRPC]
    public void RPC_UpdatePassAction(int playerId)
    {
        passed.Add(playerId);

    }

    #endregion

    #region Master-side RPC handlers (принимают решения клиентов)


    private void Update()
    {
        if (!PhotonNetwork.IsMasterClient || !isAuctionActive) return;

        double elapsed = PhotonNetwork.Time - bidStartTime;
        float timeLeft = Mathf.Max(0f, BID_DURATION - (float)elapsed);

        int currentBidder = bidders.Count > 0 ? bidders[currentBidderIndex] : -1;
        if (currentBidder != -1)
        {
            // Отправляем всем клиентам через RPC
            photonView.RPC(nameof(RPC_UpdateAuctionTimer), RpcTarget.All, currentBidder, timeLeft);
        }

        if (timeLeft <= 0f && currentBidder != -1)
        {
            PassRequest(currentBidder);
        }
    }
    [PunRPC]
    private void RPC_UpdateAuctionTimer(int currentBidderActorNumber, float timeLeft)
    {
        EventBus.Publish(new AuctionTimerUpdatedEvent(currentBidderActorNumber, timeLeft));
    }

    #endregion

    #region Master helpers

    private List<int> BuildTurnOrderStartingAfter(int starterActorNumber, HashSet<int> excludedPlayers)
    {
        var list = PhotonNetwork.PlayerList
         .OrderBy(p => p.ActorNumber)
         .Select(p => p.ActorNumber)
         .Where(p => !excludedPlayers.Contains(p)) // исключаем отказавшихся (в том числе стартера)
         .ToList();

        if (list.Count == 0)
            return new List<int>();

        int idx = list.FindIndex(x => x > starterActorNumber);
        if (idx < 0) idx = 0; // если никто не больше стартера, начинаем с начала списка

        var ordered = new List<int>();
        for (int i = 0; i < list.Count; i++)
        {
            ordered.Add(list[(idx + i) % list.Count]);
        }

        return ordered;
    }
    private void MoveToNextBidderAndPrompt()
    {
        // Если больше нет участников, а ставок не было → никто не выиграл
        if (bidders.Count == 0)
        {
            if (lastBidderActorNumber == -1)
                EndAuction_NoWinner();
            else
                EndAuction_WithWinner(lastBidderActorNumber, currentPrice);
            return;
        }

        // Получаем список оставшихся участников, которые еще не пасовали
        var activeBidders = bidders.Where(b => !passed.Contains(b)).ToList();

        // Особый случай: остался только один участник
        if (activeBidders.Count == 1)
        {
            currentBidderIndex = bidders.IndexOf(activeBidders[0]);
            PromptCurrentBidder();
            return; // ждём решения игрока (купит или пас)
        }

        // Иначе — обычная логика: найти следующего не пасовавшего
        for (int safety = 0; safety < bidders.Count; safety++)
        {
            currentBidderIndex = (currentBidderIndex + 1) % bidders.Count;
            int candidate = bidders[currentBidderIndex];
            if (!passed.Contains(candidate))
            {
                PromptCurrentBidder();
                return;
            }
        }

        // Если никто не остался
        if (lastBidderActorNumber == -1)
            EndAuction_NoWinner();
        else
            EndAuction_WithWinner(lastBidderActorNumber, currentPrice);
    }

    private void PromptCurrentBidder()
    {
        if (bidders.Count == 0)
        {
            Debug.LogWarning("[Auction] No bidders to prompt!");
            return;
        }
        int bidder = bidders[currentBidderIndex];
        int minAllowedBid = (lastBidderActorNumber == -1)
            ? basePrice + FIRST_BID_INCREMENT // первая ставка
            : currentPrice + FIRST_BID_INCREMENT; // после чьей-то ставки
        bidStartTime = PhotonNetwork.Time;

        photonView.RPC(nameof(RPC_PromptBid), RpcTarget.All, bidder, companyId, currentPrice, minAllowedBid, bidStartTime);

    }

    private void EndAuction_NoWinner()
    {
        isAuctionActive = false;
        foreach (var player in PhotonNetwork.PlayerList)
        {
            photonView.RPC(nameof(RPC_ResetAuctionTimer), RpcTarget.All, player.ActorNumber);
        }

        photonView.RPC(nameof(RPC_AuctionEnded), RpcTarget.All, -1, 0, companyId, (int)AuctionEndReason.NoBids);

    }

    private void EndAuction_WithWinner(int winnerActorNumber, int finalPrice)
    {
        isAuctionActive = false;
        foreach (var player in PhotonNetwork.PlayerList)
        {
            photonView.RPC(nameof(RPC_ResetAuctionTimer), RpcTarget.All, player.ActorNumber);
        }

        photonView.RPC(nameof(RPC_AuctionEnded), RpcTarget.All, winnerActorNumber, finalPrice, companyId, (int)AuctionEndReason.Winner);
    }
    [PunRPC]
    private void RPC_ResetAuctionTimer(int playerId)
    {
        UIAuctionWindow.Instance.HideWindow();
        EventBus.Publish(new AuctionTimerUpdatedEvent(playerId, 0f));
    }

    #endregion

    #region Client RPC → оповещения UI/логики через EventBus



    /// <summary>
    /// Приглашение сделать ставку. UI должен показываться только локальному игроку, если его ActorNumber == bidderActorNumber.
    /// </summary>
    [PunRPC]
    private void RPC_PromptBid(int bidderActorNumber, int companyId, int shownCurrentPrice, int minAllowedBid, double startTime)
    {
        EventBus.Publish(new AuctionPromptBidEvent(bidderActorNumber, companyId, shownCurrentPrice, minAllowedBid));


    }


    [PunRPC]
    private void RPC_AuctionEnded(int winnerActorNumber, int finalPrice, int companyId, int reasonInt)
    {
        var reason = (AuctionEndReason)reasonInt;
        if (reason == AuctionEndReason.Winner)
        {
            EventBus.Publish(new AuctionEndedEventWin(winnerActorNumber, finalPrice, companyId, reason));
        }
        TurnManager.Instance.OnAuctionEnded(starterId);

    }

    #endregion
}

#region Events

public enum AuctionEndReason
{
    NoBids = 0, // все пасовали, ставок не было
    Winner = 1  // есть победитель
}


/// <summary>
/// Пришёл ход делать ставку. Показывайте кнопки только если локальный игрок == BidderActorNumber.
/// </summary>

public class AuctionStartedEvent
{
    public int CompanyId { get; }
    public int StartPrice { get; }
    public List<int> Participants { get; }
    public AuctionStartedEvent(int companyId, int startPrice, List<int> participants)
    {
        CompanyId = companyId;
        StartPrice = startPrice;
        Participants = participants;
    }
}
public class AuctionTimerUpdatedEvent
{
    public int PlayerId { get; }
    public float TimeLeft { get; }
    public AuctionTimerUpdatedEvent(int playerId, float timeLeft)
    {
        PlayerId = playerId;
        TimeLeft = timeLeft;
    }
}
public class AuctionTurnStartEvent
{
    public int PlayerId { get; }
    public int CurrentPrice { get; }
    public AuctionTurnStartEvent(int playerId, int currentPrice)
    {
        PlayerId = playerId;
        CurrentPrice = currentPrice;
    }
}
public class AuctionEndedEvent
{
    public int WinnerId { get; }
    public int CompanyId { get; }
    public int FinalPrice { get; }
    public AuctionEndedEvent(int winnerId, int companyId, int finalPrice)
    {
        WinnerId = winnerId;
        CompanyId = companyId;
        FinalPrice = finalPrice;
    }
}
public class AuctionPromptBidEvent
{
    public int BidderActorNumber { get; }
    public int CompanyId { get; }
    public int ShownCurrentPrice { get; } // текущая отображаемая цена (0 → покажите базовую)
    public int MinAllowedBid { get; }     // минимальная допустимая ставка для этого хода
    public AuctionPromptBidEvent(int bidderActorNumber, int companyId, int shownCurrentPrice, int minAllowedBid)
    {
        BidderActorNumber = bidderActorNumber;
        CompanyId = companyId;
        ShownCurrentPrice = shownCurrentPrice;
        MinAllowedBid = minAllowedBid;
    }
}


public class AuctionEndedEventWin
{
    public int WinnerActorNumber { get; }
    public int FinalPrice { get; }
    public int CompanyId { get; }
    public AuctionEndReason Reason { get; }
    public AuctionEndedEventWin(int winnerActorNumber, int finalPrice, int companyId, AuctionEndReason reason)
    {
        WinnerActorNumber = winnerActorNumber;
        FinalPrice = finalPrice;
        CompanyId = companyId;
        Reason = reason;
    }
}


/// <summary>
/// Доп. доменное событие — победитель аукциона купил компанию.
/// Удобно для логов/уведомлений.
/// </summary>

#endregion