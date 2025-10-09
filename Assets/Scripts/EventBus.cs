using System;
using System.Collections.Generic;
using UnityEngine;

public class EventBus : IEventBus
{
    private readonly Dictionary<Type, List<Delegate>> eventHandlers = new();

    public void Subscribe<TEvent>(Action<TEvent> handler)
    {
        var type = typeof(TEvent);
        if (!eventHandlers.ContainsKey(type))
            eventHandlers[type] = new List<Delegate>();

        eventHandlers[type].Add(handler);
    }

    public void Unsubscribe<TEvent>(Action<TEvent> handler)
    {
        var type = typeof(TEvent);
        if (eventHandlers.ContainsKey(type))
        {
            eventHandlers[type].Remove(handler);
            if (eventHandlers[type].Count == 0)
                eventHandlers.Remove(type);
        }
    }

    public void Publish<TEvent>(TEvent eventData)
    {
        var type = typeof(TEvent);
        if (!eventHandlers.ContainsKey(type)) return;

        var handlersCopy = new List<Delegate>(eventHandlers[type]);
        foreach (var handler in handlersCopy)
        {
            try
            {
                ((Action<TEvent>)handler)?.Invoke(eventData);
            }
            catch (Exception ex)
            {
                Debug.LogError($"EventBus: ошибка в обработчике события {type.Name}: {ex}");
            }
        }
    }

}
public class EndAuctionWithWinnerEvent
{
    public int WinnerId;
    public int CompanyId;
    public int FinalPrice;
    public EndAuctionWithWinnerEvent(int winnerId, int finalPrice, int companyId)
    {
        WinnerId = winnerId;
        FinalPrice = finalPrice;
        CompanyId = companyId;
    }
}
public class PlayerBidAuction
{
    public int PlayerId;
    public PlayerBidAuction(int playerId)
    {
        PlayerId = playerId;
    }
}
public class PlayerOccupancyRegisterEvent
{
    public int CellIndex;
    public PlayerMove PlayerMove;

    public PlayerOccupancyRegisterEvent(int cellIndex, PlayerMove playerMove)
    {
        CellIndex = cellIndex;
        PlayerMove = playerMove;
    }
}

public class PlayerOccupancyUnregisterEvent
{
    public int CellIndex;
    public PlayerMove PlayerMove;

    public PlayerOccupancyUnregisterEvent(int cellIndex, PlayerMove playerMove)
    {
        CellIndex = cellIndex;
        PlayerMove = playerMove;
    }
}


public class PlayerJoinedEvent
{
    public PlayerData Player { get; }

    public PlayerJoinedEvent(PlayerData player)
    {
        Player = player;
    }
}
public class LapMoneyEvent
{
    public int playerId;
    public int currentCellId;
    public int targetCellId;
    public bool isForward;

    public LapMoneyEvent(int playerId, int currentCellId, int targetCellId, bool isForward)
    {
        this.playerId = playerId;
        this.currentCellId = currentCellId;
        this.targetCellId = targetCellId;
        this.isForward = isForward;
    }
}
public class OfferLoanPayEvent
{
    public int PlayerId { get; }
    public int LoanAmount { get; }

    public OfferLoanPayEvent(int playerId, int loanAmount)
    {
        PlayerId = playerId;
        LoanAmount = loanAmount;
    }
}
public class CompanyMortgagedEvent
{
    public int PlayerId { get; }
    public int CompanyId { get; }
    public int Turns { get; }

    public CompanyMortgagedEvent(int playerId, int companyId, int turns)
    {
        PlayerId = playerId;
        CompanyId = companyId;
        Turns = turns;
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
public class StartAuctionEvent
{
    public int StarterActorNumber;
    public int CompanyId;
    public int CompanyBasePrice;
    public StartAuctionEvent(int starterActorNumber, int companyId, int companyBasePrice)
    {
        StarterActorNumber = starterActorNumber;
        CompanyId = companyId;
        CompanyBasePrice = companyBasePrice;
    }
}
public class AuctionPromptBidEvent
{
    public int PlayerId { get; }
    public int Bid { get; }
    public int CompanyId { get; }
    public AuctionPromptBidEvent(int playerId, int bid, int companyId)
    {
        PlayerId = playerId;
        Bid = bid;
        CompanyId = companyId;
    }
}
public class AuctionEndEvent
{

}
public class PlayerPassAuction
{
    public int PlayerId;
    public PlayerPassAuction(int playerId)
    {
        PlayerId = playerId;
    }
}

public class CompanyTickUIEvent
{
    public int CompanyId { get; }

    public int TurnsLeft { get; }
    public CompanyTickUIEvent(int companyId, int turnsLeft)
    {
        CompanyId = companyId;
        TurnsLeft = turnsLeft;
    }
}

public class MoveToJailEvent
{
    public int PlayerID;

    public MoveToJailEvent(int playerId)
    {
        PlayerID = playerId;
    }
}

public class HandleCellEvent
{
    public int CellID;
    public int PlayerID;

    public HandleCellEvent(int cellID, int playerId)
    {
        CellID = cellID;
        PlayerID = playerId;
    }
}
public class MovePlayerEvent
{
    public int PlayerId;
    public int CurrentCellIndex;
    public int Steps;
    public bool IsForward;
    public int TargetIndex;
    public MovePlayerEvent(int playerId, int currentCellIndex, int steps, bool isForward, int targetIndex)
    {
        PlayerId = playerId;
        CurrentCellIndex = currentCellIndex;
        Steps = steps;
        IsForward = isForward;
        TargetIndex = targetIndex;
    }
}
public class SetTurnsJailEvent
{
    public int Turns;
    public int PlayerID;

    public SetTurnsJailEvent(int playerID, int turns)
    {
        Turns = turns;
        PlayerID = playerID;
    }
}

public class OnTakeLoanEvent
{
    public PlayerData PlayerData;

    public OnTakeLoanEvent(PlayerData playerData)
    {
        PlayerData = playerData;
    }
}

public class OnStartTurnLoanEvent
{
    public int PlayerId;

    public OnStartTurnLoanEvent(int playerId)
    {
        PlayerId = playerId;
    }
}
public class DiceRolledEvent
{
    public DiceResult DiceResult { get; }
    public int PlayerId { get; }
    public bool IsForJail { get; }

    public DiceRolledEvent(DiceResult diceResult, int playerId, bool isForJail)
    {
        DiceResult = diceResult;
        PlayerId = playerId;
        IsForJail = isForJail;
    }
}

public class TradeStartedEvent
{
    public int FromPlayerId { get; }
    public int ToPlayerId { get; }
    public TradeOffer Offer { get; }

    public TradeStartedEvent(int from, int to, TradeOffer offer)
    { FromPlayerId = from; ToPlayerId = to; Offer = offer; }
}

public class TradeUpdatedEvent
{
    public TradeOffer Offer { get; }

    public TradeUpdatedEvent(TradeOffer offer)
    { Offer = offer; }
}


public class TradeProposalReceivedEvent
{
    public TradeOffer Offer { get; }

    public TradeProposalReceivedEvent(TradeOffer offer, int from, int to)
    {
        Offer = offer;
    }
}

public class TradeEndedEvent
{
    public bool Accepted { get; }
    public int SenderId { get; }
    public int ReceiverId { get; }

    public TradeEndedEvent(bool accepted, int senderId, int receiverId)
    { Accepted = accepted; SenderId = senderId; ReceiverId = receiverId; }
}
public class CompanyUIAction
{
    public int CompanyId { get; }
    public CompanyActionType ActionType { get; }

    public CompanyUIAction(int companyId, CompanyActionType actionType)
    {
        CompanyId = companyId;
        ActionType = actionType;
    }
}
public class TimerUpdatedEvent
{
    public TimerType Type { get; }
    public int PlayerId { get; }
    public float TimeLeft { get; }
    public bool IsActive { get; }

    public TimerUpdatedEvent(TimerType type, int playerId, float timeLeft, bool isActive)
    {
        Type = type;
        PlayerId = playerId;
        TimeLeft = timeLeft;
        IsActive = isActive;
    }
}
public class TimerExpiredEvent
{
    public TimerType Type { get; }
    public int PlayerId { get; }

    public TimerExpiredEvent(TimerType type, int playerId)
    {
        Type = type;
        PlayerId = playerId;
    }
}
public class OfferRentEvent
{
    public int CellIndex;
    public int PlayerId;
    public int Rent;
    public OfferRentEvent(int cellIndex, int playerId, int rent)
    {
        CellIndex = cellIndex;
        PlayerId = playerId;
        Rent = rent;
    }
}
public class CompanyBoughtEvent
{
    public int CellIndex;
    public int PlayerId;
    public CompanyBoughtEvent(int cellIndex, int playerId)
    {
        CellIndex = cellIndex;
        PlayerId = playerId;
    }
}
public class RentPaidEvent
{
    public int CellIndex;
    public int PlayerId;
    public int Owner;
    public int Rent;
    public RentPaidEvent(int cellIndex, int playerId, int owner, int rent)
    {
        CellIndex = cellIndex;
        PlayerId = playerId;
        Owner = owner;
        Rent = rent;
    }
}
public class OfferPurchaseEvent
{
    public int CellIndex;
    public int PlayerId;
    public int Price;
    public bool CanAfford;
    public OfferPurchaseEvent(int cellIndex, int playerId, int price, bool canAfford)
    {
        CellIndex = cellIndex;
        PlayerId = playerId;
        Price = price;
        CanAfford = canAfford;
    }
}

public class HideButtonsTradeEvent
{
    public int CompanyId;

    public HideButtonsTradeEvent(int CompanyId)
    {
        this.CompanyId = CompanyId;
    }
}
public class TurnStartEvent
{
    public int PlayerId;

    public TurnStartEvent(int playerId)
    {
        PlayerId = playerId;
    }
}

public class StartTurnJailEvent
{
    public int PlayerId;

    public StartTurnJailEvent(int playerId)
    {
        PlayerId = playerId;
    }
}

public class OnUpdatePlayerMoneyEvent
{
    public PlayerData Player;
    public OnUpdatePlayerMoneyEvent(PlayerData player)
    {
        Player = player;
    }

}
public class ShowCompanyWindowEvent
{
    public RectTransform Cell { get; }
    public StatsWindowPosition Position { get; }
    public CompanyData Data { get; }

    public ShowCompanyWindowEvent(RectTransform cell, StatsWindowPosition pos, CompanyData data)
    {
        Cell = cell;
        Position = pos;
        Data = data;
    }
}

public class ShowFieldCompanyWindowEvent
{
    public RectTransform Cell { get; }
    public StatsWindowPosition Position { get; }
    public FieldCompanyData Data { get; }

    public ShowFieldCompanyWindowEvent(RectTransform cell, StatsWindowPosition pos, FieldCompanyData data)
    {
        Cell = cell;
        Position = pos;
        Data = data;
    }
}

public class ShowDiceCompanyWindowEvent
{
    public RectTransform Cell { get; }
    public StatsWindowPosition Position { get; }
    public DiceCompanyData Data { get; }

    public ShowDiceCompanyWindowEvent(RectTransform cell, StatsWindowPosition pos, DiceCompanyData data)
    {
        Cell = cell;
        Position = pos;
        Data = data;
    }
}

public class OnPlayerMoveEvent
{
    public int PlayerId { get; }
    public int Steps { get; }
    public bool Forward { get; }
    public OnPlayerMoveEvent(int playerId, int steps, bool forward)
    {
        PlayerId = playerId;
        Steps = steps;
        Forward = forward;
    }
}
public class DiceFadeEvent
{

    public int CellId;
    public bool IsMovementStart;
    public DiceFadeEvent(int cellId, bool isMovementStart)
    {

        CellId = cellId;
        IsMovementStart = isMovementStart;
    }
}

