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
    private IBankService bankService;
    private IEventBus eventBus;
    private GameSettings gameSettings;
    [Inject]
    public void Construct(IPlayerRepository playerRepository, IPhotonLoanManager network, IBankService bankService, IEventBus eventBus, GameSettings gameSettings)
    {
        this.playerRepository = playerRepository;
        this.photonLoanManager = network;
        this.bankService = bankService; 
        this.eventBus = eventBus;
        this.gameSettings = gameSettings;
    }
    void IInitializable.Initialize()
    {
        eventBus.Subscribe<OnStartTurnLoanEvent>(OnPlayerTurnStart);
    }
    void IDisposable.Dispose()
    {
        eventBus.Unsubscribe<OnStartTurnLoanEvent>(OnPlayerTurnStart);
    }
    public void RequestTakeLoan(int playerId)
    {
        var player = playerRepository.GetPlayerById(playerId);
        if (player.HasLoan) return;

        photonLoanManager.TakeLoanRequest(playerId);
    }

    public void RequestPayLoan(int playerId)
    {
        var player = playerRepository.GetPlayerById(playerId);
        if (!player.HasLoan) return;

        photonLoanManager.PayLoanRequest(playerId);
    }

    private void OnPlayerTurnStart(OnStartTurnLoanEvent e)
    {
        var player = playerRepository.GetPlayerById(e.PlayerId);
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

    // Этот метод вызывается **только из RPC**
    public void TakeLoanConfirmed(int playerId)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            bankService.AddMoney(playerId, gameSettings.loanAmount);
        }
        var player = playerRepository.GetPlayerById(playerId);

        player.HasLoan = true;
        player.LoanTurnsLeft = 1;

        eventBus.Publish(new OnTakeLoanEvent(player));
        eventBus.Publish(new OnUpdatePlayerMoneyEvent(player));
    }

    public void PayLoanConfirmed(int playerId)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            bankService.RemoveMoney(playerId, gameSettings.loanAmountBack);
        }
        var player = playerRepository.GetPlayerById(playerId);

        player.HasLoan = false;
        player.LoanTurnsLeft = 0;

        eventBus.Publish(new OnTakeLoanEvent(player));
        eventBus.Publish(new OnUpdatePlayerMoneyEvent(player));
    }
}
