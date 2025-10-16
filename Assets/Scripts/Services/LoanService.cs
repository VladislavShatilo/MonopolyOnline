using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class LoanService : ILoanService,IInitializable,IDisposable
{
    private IPlayerRepository playerRepository;
    private IPhotonLoanManager photonLoanManager;
    private IPhotonNetworkWrapper photonNetworkWrapper;

    private IBankService bankService;
    private IEventBus eventBus;
    private GameSettings gameSettings;

    #region LIFE_CYCLE

    [Inject]
    public void Construct(IPlayerRepository playerRepository, IPhotonLoanManager photonLoanManager, IBankService bankService, IEventBus eventBus, GameSettings gameSettings, IPhotonNetworkWrapper photonNetworkWrapper)
    {
        this.playerRepository = playerRepository ?? throw new ArgumentNullException(nameof(playerRepository));
        this.photonLoanManager = photonLoanManager ?? throw new ArgumentNullException(nameof(photonLoanManager));
        this.bankService = bankService ?? throw new ArgumentNullException(nameof(bankService));
        this.eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        this.gameSettings = gameSettings ?? throw new ArgumentNullException(nameof(gameSettings));
        this.photonNetworkWrapper = photonNetworkWrapper ?? throw new ArgumentNullException(nameof(photonNetworkWrapper));

    }
    public void Initialize()
    {
        eventBus.Subscribe<OnStartTurnLoanEvent>(OnPlayerTurnStart);
    }
    public void Dispose()
    {
        eventBus.Unsubscribe<OnStartTurnLoanEvent>(OnPlayerTurnStart);
    }

    #endregion LIFE_CYCLE

    #region PUBLIC_METHODS

    public void TakeLoanConfirmed(int playerId)
    {
        if (photonNetworkWrapper.IsMasterClient)
        {
            bankService.AddMoney(playerId, gameSettings.loanAmount);
        }
        var player = playerRepository.GetPlayerById(playerId) ?? throw new InvalidOperationException(nameof(TakeLoanConfirmed));

        player.HasLoan = true;
        player.LoanTurnsLeft = 1;

        eventBus.Publish(new OnTakeLoanEvent(player));
        eventBus.Publish(new OnUpdatePlayerMoneyEvent(player));
    }

    public void PayLoanConfirmed(int playerId)
    {
        if (photonNetworkWrapper.IsMasterClient)
        {
            bankService.RemoveMoney(playerId, gameSettings.loanAmountBack);
        }
        var player = playerRepository.GetPlayerById(playerId) ?? throw new InvalidOperationException(nameof(PayLoanConfirmed));

        player.HasLoan = false;
        player.LoanTurnsLeft = 0;

        eventBus.Publish(new OnTakeLoanEvent(player));
        eventBus.Publish(new OnUpdatePlayerMoneyEvent(player));
    }

    #endregion PUBLIC_METHODS

    #region CALLBACKS

    private void OnPlayerTurnStart(OnStartTurnLoanEvent e)
    {
        var player = playerRepository.GetPlayerById(e.PlayerId) ?? throw new InvalidOperationException(nameof(OnPlayerTurnStart));
        if (!player.HasLoan) return;

        player.LoanTurnsLeft--;

        if (player.LoanTurnsLeft <= 0)
        {
            photonLoanManager.ShowLoanWindow(e.PlayerId, gameSettings.loanAmountBack);
        }
        else
        {
            eventBus.Publish(new OnTakeLoanEvent(player));
        }
    }

    #endregion CALLBACKS

}
