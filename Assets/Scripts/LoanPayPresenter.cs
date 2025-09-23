using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class LoanPayPresenter : IInitializable, IDisposable
{
    private ILoanPayWindow loanPayWindow;
    private ILocalPlayerService localPlayerService;
    private IEventBus eventBus;
    private IBankService bankService;
    private IPhotonLoanManager photonLoanManager;
    [Inject]
    public void Construct(ILoanPayWindow loanPayWindow, ILocalPlayerService localPlayerService,IEventBus eventBus, IBankService bankService, IPhotonLoanManager photonLoanManager)
    {
        Debug.Log("Construct");
        this.loanPayWindow = loanPayWindow;
        this.localPlayerService = localPlayerService;
        this.eventBus = eventBus;
        this.bankService = bankService;
        this.photonLoanManager = photonLoanManager;
    }

    void IInitializable.Initialize()
    {
        Debug.Log("Initialize");

        eventBus.Subscribe<OfferLoanPayEvent>(ShowLoanWindow);
        loanPayWindow.SetPayLoanAction(OnPayLoan);
    }

    void IDisposable.Dispose()
    {
        eventBus.Unsubscribe<OfferLoanPayEvent>(ShowLoanWindow);
    }

    private void ShowLoanWindow(OfferLoanPayEvent e)
    {
        int localId = localPlayerService.GetLocalPlayerId();
        if (e.PlayerId == localId)
        {
            bool canAfford = bankService.HasEnoughMoney(e.PlayerId, e.LoanAmount);
            loanPayWindow.Show(e.PlayerId, e.LoanAmount, canAfford);
        }
        else
        {
            loanPayWindow.Hide();
        }
    }

    private void OnPayLoan(int playerId)
    {
        photonLoanManager.PayLoanRequest(playerId);
        loanPayWindow.Hide();
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