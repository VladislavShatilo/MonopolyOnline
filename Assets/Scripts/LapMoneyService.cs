using System;
using UnityEditor;
using UnityEngine;
using Zenject;

public class LapMoneyService : IInitializable, IDisposable
{
    private IEventBus eventBus;
    private IBankService bankService;
    private GameSettings gameSettings;
    private IChatService chatService;
    
    [Inject]
    public void Construct(IEventBus eventBus, IBankService bankService, GameSettings gameSettings, IChatService chatService)
    {
        this.eventBus = eventBus;
        this.bankService = bankService;
        this.gameSettings = gameSettings;
        this.chatService = chatService;
    }


    void IInitializable.Initialize()
    {
        eventBus.Subscribe<LapMoneyEvent>(GiveMoneyLap);
    }
    void IDisposable.Dispose()
    {
        eventBus.Unsubscribe<LapMoneyEvent>(GiveMoneyLap);

    }

    private void GiveMoneyLap(LapMoneyEvent e)
    {
        if (e.isForward && e.targetCellId < e.currentCellId)
        {
            bankService.AddMoney(e.playerId, gameSettings.lapMoney);
            string message = $"Игрок {e.playerId} прошёл круг и получил {gameSettings.lapMoney}$!";
            chatService.SendMessage(e.playerId, message, false);
        }
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
