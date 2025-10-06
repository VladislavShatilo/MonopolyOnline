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

    #region LIFE_CYCLE

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

    #endregion LIFE_CYCLE

    #region CALLBACKS

    private void GiveMoneyLap(LapMoneyEvent e)
    {
        if (e.isForward && e.targetCellId < e.currentCellId)
        {
            bankService.AddMoney(e.playerId, gameSettings.lapMoney);
            string message = $"Игрок {e.playerId} прошёл круг и получил {gameSettings.lapMoney}$!";
            chatService.SendMessage(e.playerId, message, false);
        }
    }

    #endregion CALLBACKS

}
