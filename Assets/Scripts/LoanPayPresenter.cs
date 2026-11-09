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

    #region LIFE_CYCLE

    [Inject]
    public void Construct(ILoanPayWindow loanPayWindow, ILocalPlayerService localPlayerService, IEventBus eventBus, IBankService bankService, IPhotonLoanManager photonLoanManager)
    {
        this.loanPayWindow = loanPayWindow ?? throw new ArgumentNullException(nameof(loanPayWindow));
        this.localPlayerService = localPlayerService ?? throw new ArgumentNullException(nameof(localPlayerService));
        this.eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        this.bankService = bankService ?? throw new ArgumentNullException(nameof(bankService));
        this.photonLoanManager = photonLoanManager ?? throw new ArgumentNullException(nameof(photonLoanManager));
    }

    public void Initialize()
    {
        eventBus.Subscribe<OfferLoanPayEvent>(ShowLoanWindow);

        loanPayWindow.SetPayLoanAction(OnPayLoan);
    }

    public void Dispose()
    {
        eventBus.Unsubscribe<OfferLoanPayEvent>(ShowLoanWindow);
    }

    #endregion LIFE_CYCLE

    #region CALLBACKS

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

    #endregion CALLBACKS
}